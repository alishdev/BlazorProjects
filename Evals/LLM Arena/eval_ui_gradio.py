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
        return f"Question {current_question_number}/{total}: {question['question_text']}"
    
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
    
    def get_first_question():
        """Get the first question (ungraded or graded)"""
        if ungraded:
            return ungraded[0]
        elif graded:
            return graded[0]
        return None
    
    def get_last_question():
        """Get the last question (graded or ungraded)"""
        if graded:
            return graded[-1]
        elif ungraded:
            return ungraded[-1]
        return None
    
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
        gr.Markdown(f"**Graded {graded_count} / {total}**")
        
        # Navigation buttons
        with gr.Row():
            first_btn = gr.Button("First", variant="secondary")
            last_btn = gr.Button("Last", variant="secondary")
        
        # Question display
        question_display = gr.Markdown(get_question_info(current_question))
        
        # Answer selection - each answer as an option
        llm_names, answer_options = get_answers_display(current_question)
        selected_answer = get_selected_answer(current_question)
        
        answer_radio = gr.Radio(
            choices=answer_options,
            label="",
            value=selected_answer
        )
        
        # Save button
        save_btn = gr.Button("Select as Best", variant="primary")
        status_output = gr.Textbox(label="Status", interactive=False)
        
        # Navigation functions
        def on_first_click():
            question = get_first_question()
            if question:
                llm_names, answer_options = get_answers_display(question)
                selected_answer = get_selected_answer(question)
                return (
                    get_question_info(question),
                    gr.Radio(choices=answer_options, value=selected_answer),
                    ""
                )
            return question_display.value, answer_radio.value, "No questions available."
        
        def on_last_click():
            question = get_last_question()
            if question:
                llm_names, answer_options = get_answers_display(question)
                selected_answer = get_selected_answer(question)
                return (
                    get_question_info(question),
                    gr.Radio(choices=answer_options, value=selected_answer),
                    ""
                )
            return question_display.value, answer_radio.value, "No questions available."
        
        def on_save_click(selected_answer):
            if not selected_answer:
                return "Please select an answer first."
            
            result = save_selection(current_question, selected_answer)
            return result
        
        # Connect events
        first_btn.click(
            fn=on_first_click,
            outputs=[question_display, answer_radio, status_output]
        )
        
        last_btn.click(
            fn=on_last_click,
            outputs=[question_display, answer_radio, status_output]
        )
        
        save_btn.click(
            fn=on_save_click,
            inputs=[answer_radio],
            outputs=[status_output]
        )
    
    return demo 