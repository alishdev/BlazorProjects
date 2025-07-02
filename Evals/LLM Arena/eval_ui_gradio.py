import os
import json
import sqlite3
import gradio as gr

def launch_evaluation_ui(config):
    """Launch the human-in-the-loop evaluation UI (Gradio)."""
    output_data_path = config['output_data_path']
    db_path = config['database_path']
    answers_path = os.path.join(output_data_path, "generated_answers.json")
    
    if not os.path.exists(answers_path):
        return "Error: generated_answers.json not found. Run generate_answers first."
    
    with open(answers_path, "r", encoding="utf-8") as f:
        data = json.load(f)
    
    # Connect to DB and ensure table exists
    conn = sqlite3.connect(db_path)
    c = conn.cursor()
    c.execute('''CREATE TABLE IF NOT EXISTS evaluation_results (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        question_id TEXT NOT NULL,
        question_text TEXT NOT NULL,
        best_answer_text TEXT NOT NULL,
        winning_llm_name TEXT NOT NULL,
        evaluation_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    )''')
    conn.commit()
    conn.close()
    
    # Load graded question_ids
    conn = sqlite3.connect(db_path)
    c = conn.cursor()
    c.execute("SELECT question_id FROM evaluation_results")
    graded_ids = set(row[0] for row in c.fetchall())
    conn.close()
    
    # Find ungraded and graded questions
    ungraded = [q for q in data if q['question_id'] not in graded_ids]
    graded = [q for q in data if q['question_id'] in graded_ids]
    
    total = len(data)
    graded_count = len(graded)
    
    def get_question_info(question):
        """Get question information for display"""
        current_question_number = data.index(question) + 1
        return f"**Question {current_question_number}/{total}:**\n\n{question['question_text']}"
    
    def get_answers_display(question):
        """Get answers for display"""
        answers = question['answers']
        llm_names = list(answers.keys())
        answer_options = [f"{name}: {answer}" for name, answer in answers.items()]
        return llm_names, answer_options
    
    def get_selected_answer(question):
        """Get the previously selected answer for this question if it exists"""
        conn = sqlite3.connect(db_path)
        c = conn.cursor()
        c.execute("SELECT winning_llm_name FROM evaluation_results WHERE question_id = ?", (question['question_id'],))
        result = c.fetchone()
        conn.close()
        if result:
            winning_llm = result[0]
            return f"{winning_llm}: {question['answers'][winning_llm]}"
        return None
    
    def save_selection(question, selected_answer):
        """Save the selected answer to database"""
        if not selected_answer:
            return "Please select an answer first."
        
        # Extract LLM name from the selected answer
        llm_name = selected_answer.split(": ")[0]
        
        conn = sqlite3.connect(db_path)
        c = conn.cursor()
        
        # Remove existing evaluation if it exists
        c.execute("DELETE FROM evaluation_results WHERE question_id = ?", (question['question_id'],))
        
        # Insert new evaluation
        c.execute(
            "INSERT INTO evaluation_results (question_id, question_text, best_answer_text, winning_llm_name) VALUES (?, ?, ?, ?)",
            (question['question_id'], question['question_text'], question['answers'][llm_name], llm_name)
        )
        conn.commit()
        conn.close()
        return "Result saved!"
    
    def get_default_question():
        """Get the default question to show"""
        if not ungraded:
            # All questions graded - show the last question
            if graded:
                return graded[-1]
            else:
                return None
        else:
            # Show first ungraded question
            return ungraded[0]
    
    # Get initial question
    current_question = get_default_question()
    if not current_question:
        return "No questions available."
    
    # Create Gradio interface
    with gr.Blocks(title="LLM Arena: Human Evaluation") as demo:
        gr.Markdown(f"# LLM Arena: Human Evaluation")
        
        # State to track current question
        current_question_state = gr.State(current_question)
        
        # Question display
        question_display = gr.Markdown(get_question_info(current_question))
        
        # Navigation buttons
        with gr.Row():
            first_btn = gr.Button("First", variant="secondary")
            prev_btn = gr.Button("Prev", variant="secondary", interactive=False)
            next_btn = gr.Button("Next", variant="secondary", interactive=False)
            last_btn = gr.Button("Last", variant="secondary")
        
        # Answer selection - each answer as an option
        llm_names, answer_options = get_answers_display(current_question)
        selected_answer = get_selected_answer(current_question)
        
        # Create a container for answers with buttons and markdown
        answer_container = gr.Column()
        
        # Create buttons and markdown for each answer option
        answer_buttons = []
        answer_displays = []
        
        for i, option in enumerate(answer_options):
            is_selected = (option == selected_answer)
            llm_name = llm_names[i]  # Get the LLM name for this option
            
            with answer_container:
                with gr.Row():
                    # Small selection button with LLM name
                    btn = gr.Button(
                        llm_name,
                        variant="primary" if is_selected else "secondary",
                        size="sm",
                        min_width=30
                    )
                    answer_buttons.append(btn)
                    
                    # Full answer text in markdown (without LLM name)
                    answer_text = option.split(": ", 1)[1] if ": " in option else option
                    answer_text = gr.Markdown(answer_text)
                    answer_displays.append(answer_text)
        
        # Navigation functions
        def update_button_states(current_q):
            """Update the enabled/disabled state of navigation buttons"""
            current_index = data.index(current_q)
            selected_answer = get_selected_answer(current_q)
            
            # Prev button: disabled on first question
            prev_enabled = current_index > 0
            
            # Next button: disabled on last question or if no answer selected
            next_enabled = current_index < len(data) - 1 and selected_answer is not None
            
            return prev_enabled, next_enabled
        
        def on_answer_click(button_index, current_q):
            """Handle when user clicks an answer button - save and move to next"""
            if button_index is None:
                return question_display.value, current_q, *answer_buttons, *answer_displays
            
            # Get the current question's answer options
            llm_names, answer_options = get_answers_display(current_q)
            
            # Get the selected answer
            selected_answer = answer_options[button_index]
            
            # Save the selection for current question
            save_selection(current_q, selected_answer)
            
            # Find next question
            current_index = data.index(current_q)
            next_index = current_index + 1
            
            if next_index < len(data):
                next_question = data[next_index]
                llm_names, next_answer_options = get_answers_display(next_question)
                next_selected_answer = get_selected_answer(next_question)
                
                # Create new buttons and displays for next question
                new_buttons = []
                new_displays = []
                for i, option in enumerate(next_answer_options):
                    is_selected = (option == next_selected_answer)
                    new_buttons.append(
                        gr.Button(
                            llm_names[i],
                            variant="primary" if is_selected else "secondary",
                            size="sm",
                            min_width=30
                        )
                    )
                    # Remove LLM name from markdown text
                    answer_text = option.split(": ", 1)[1] if ": " in option else option
                    new_displays.append(gr.Markdown(answer_text))
                
                # Update button states
                prev_enabled, next_enabled = update_button_states(next_question)
                
                return (
                    get_question_info(next_question),
                    next_question,
                    gr.Button("Prev", variant="secondary", interactive=prev_enabled),
                    gr.Button("Next", variant="secondary", interactive=next_enabled),
                    *new_buttons,
                    *new_displays
                )
            else:
                # No more questions - update current question's buttons to show selection
                llm_names, answer_options = get_answers_display(current_q)
                selected_answer = get_selected_answer(current_q)
                
                # Create updated buttons for current question
                updated_buttons = []
                updated_displays = []
                for i, option in enumerate(answer_options):
                    is_selected = (option == selected_answer)
                    updated_buttons.append(
                        gr.Button(
                            llm_names[i],
                            variant="primary" if is_selected else "secondary",
                            size="sm",
                            min_width=30
                        )
                    )
                    # Remove LLM name from markdown text
                    answer_text = option.split(": ", 1)[1] if ": " in option else option
                    updated_displays.append(gr.Markdown(answer_text))
                
                # Update button states
                prev_enabled, next_enabled = update_button_states(current_q)
                
                return (
                    question_display.value,
                    current_q,
                    gr.Button("Prev", variant="secondary", interactive=prev_enabled),
                    gr.Button("Next", variant="secondary", interactive=next_enabled),
                    *updated_buttons,
                    *updated_displays
                )
        
        def on_first_click(current_q):
            # Go to the actual first question in the dataset
            first_question = data[0]
            llm_names, answer_options = get_answers_display(first_question)
            selected_answer = get_selected_answer(first_question)
            
            # Create buttons and displays for first question
            new_buttons = []
            new_displays = []
            for i, option in enumerate(answer_options):
                is_selected = (option == selected_answer)
                new_buttons.append(
                    gr.Button(
                        llm_names[i],
                        variant="primary" if is_selected else "secondary",
                        size="sm",
                        min_width=30
                    )
                )
                # Remove LLM name from markdown text
                answer_text = option.split(": ", 1)[1] if ": " in option else option
                new_displays.append(gr.Markdown(answer_text))
            
            # Update button states
            prev_enabled, next_enabled = update_button_states(first_question)
            
            return (
                get_question_info(first_question),
                first_question,
                gr.Button("Prev", variant="secondary", interactive=prev_enabled),
                gr.Button("Next", variant="secondary", interactive=next_enabled),
                *new_buttons,
                *new_displays
            )
        
        def on_last_click(current_q):
            # Go to the actual last question in the dataset
            last_question = data[-1]
            llm_names, answer_options = get_answers_display(last_question)
            selected_answer = get_selected_answer(last_question)
            
            # Create buttons and displays for last question
            new_buttons = []
            new_displays = []
            for i, option in enumerate(answer_options):
                is_selected = (option == selected_answer)
                new_buttons.append(
                    gr.Button(
                        llm_names[i],
                        variant="primary" if is_selected else "secondary",
                        size="sm",
                        min_width=30
                    )
                )
                # Remove LLM name from markdown text
                answer_text = option.split(": ", 1)[1] if ": " in option else option
                new_displays.append(gr.Markdown(answer_text))
            
            # Update button states
            prev_enabled, next_enabled = update_button_states(last_question)
            
            return (
                get_question_info(last_question),
                last_question,
                gr.Button("Prev", variant="secondary", interactive=prev_enabled),
                gr.Button("Next", variant="secondary", interactive=next_enabled),
                *new_buttons,
                *new_displays
            )
        
        def on_prev_click(current_q):
            # Go to the previous question in the dataset
            current_index = data.index(current_q)
            if current_index > 0:
                prev_question = data[current_index - 1]
                llm_names, answer_options = get_answers_display(prev_question)
                selected_answer = get_selected_answer(prev_question)
                
                # Create buttons and displays for previous question
                new_buttons = []
                new_displays = []
                for i, option in enumerate(answer_options):
                    is_selected = (option == selected_answer)
                    new_buttons.append(
                        gr.Button(
                            llm_names[i],
                            variant="primary" if is_selected else "secondary",
                            size="sm",
                            min_width=30
                        )
                    )
                    # Remove LLM name from markdown text
                    answer_text = option.split(": ", 1)[1] if ": " in option else option
                    new_displays.append(gr.Markdown(answer_text))
                
                # Update button states
                prev_enabled, next_enabled = update_button_states(prev_question)
                
                return (
                    get_question_info(prev_question),
                    prev_question,
                    gr.Button("Prev", variant="secondary", interactive=prev_enabled),
                    gr.Button("Next", variant="secondary", interactive=next_enabled),
                    *new_buttons,
                    *new_displays
                )
            else:
                # Already at first question, stay on current
                return question_display.value, current_q, prev_btn, next_btn, *answer_buttons, *answer_displays
        
        def on_next_click(current_q):
            # Go to the next question in the dataset
            current_index = data.index(current_q)
            if current_index < len(data) - 1:
                next_question = data[current_index + 1]
                llm_names, answer_options = get_answers_display(next_question)
                selected_answer = get_selected_answer(next_question)
                
                # Create buttons and displays for next question
                new_buttons = []
                new_displays = []
                for i, option in enumerate(answer_options):
                    is_selected = (option == selected_answer)
                    new_buttons.append(
                        gr.Button(
                            llm_names[i],
                            variant="primary" if is_selected else "secondary",
                            size="sm",
                            min_width=30
                        )
                    )
                    # Remove LLM name from markdown text
                    answer_text = option.split(": ", 1)[1] if ": " in option else option
                    new_displays.append(gr.Markdown(answer_text))
                
                # Update button states
                prev_enabled, next_enabled = update_button_states(next_question)
                
                return (
                    get_question_info(next_question),
                    next_question,
                    gr.Button("Prev", variant="secondary", interactive=prev_enabled),
                    gr.Button("Next", variant="secondary", interactive=next_enabled),
                    *new_buttons,
                    *new_displays
                )
            else:
                # Already at last question, stay on current
                return question_display.value, current_q, prev_btn, next_btn, *answer_buttons, *answer_displays
        
        # Connect events
        def create_click_handler(button_index):
            def click_handler(current_q):
                return on_answer_click(button_index, current_q)
            return click_handler
        
        for i, btn in enumerate(answer_buttons):
            btn.click(
                fn=create_click_handler(i),
                inputs=[current_question_state],
                outputs=[question_display, current_question_state, prev_btn, next_btn, *answer_buttons, *answer_displays]
            )
        
        first_btn.click(
            fn=on_first_click,
            inputs=[current_question_state],
            outputs=[question_display, current_question_state, prev_btn, next_btn, *answer_buttons, *answer_displays]
        )
        
        last_btn.click(
            fn=on_last_click,
            inputs=[current_question_state],
            outputs=[question_display, current_question_state, prev_btn, next_btn, *answer_buttons, *answer_displays]
        )
        
        prev_btn.click(
            fn=on_prev_click,
            inputs=[current_question_state],
            outputs=[question_display, current_question_state, prev_btn, next_btn, *answer_buttons, *answer_displays]
        )
        
        next_btn.click(
            fn=on_next_click,
            inputs=[current_question_state],
            outputs=[question_display, current_question_state, prev_btn, next_btn, *answer_buttons, *answer_displays]
        )
    
    return demo 