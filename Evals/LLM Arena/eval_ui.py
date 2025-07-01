import os
import json
import sqlite3
import streamlit as st

def launch_evaluation_ui(config):
    """Launch the human-in-the-loop evaluation UI (Streamlit)."""
    output_data_path = config['output_data_path']
    db_path = config['database_path']
    answers_path = os.path.join(output_data_path, "generated_answers.json")
    if not os.path.exists(answers_path):
        st.error("generated_answers.json not found. Run generate_answers first.")
        return
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
    # Load graded question_ids
    c.execute("SELECT question_id FROM evaluation_results")
    graded_ids = set(row[0] for row in c.fetchall())
    # Find ungraded and graded questions
    ungraded = [q for q in data if q['question_id'] not in graded_ids]
    graded = [q for q in data if q['question_id'] in graded_ids]
    total = len(data)
    graded_count = len(graded)
    st.title("LLM Arena: Human Evaluation")
    
    # Initialize session state for navigation
    if 'show_first' not in st.session_state:
        st.session_state.show_first = False
    if 'show_last' not in st.session_state:
        st.session_state.show_last = False
    
    # Navigation buttons
    col1, col2 = st.columns(2)
    with col1:
        if st.button("First"):
            st.session_state.show_first = True
            st.session_state.show_last = False
            st.rerun()
    with col2:
        if st.button("Last"):
            st.session_state.show_last = True
            st.session_state.show_first = False
            st.rerun()
    
    # Determine which question to show
    if st.session_state.show_first:
        if ungraded:
            q = ungraded[0]
        elif graded:
            q = graded[0]
        st.session_state.show_first = False
    elif st.session_state.show_last:
        if graded:
            q = graded[-1]
        elif ungraded:
            q = ungraded[-1]
        st.session_state.show_last = False
    elif not ungraded:
        # All questions graded - show the last question
        if graded:
            q = graded[-1]
        else:
            conn.close()
            return
    else:
        # Show first ungraded question
        q = ungraded[0]
    
    # Calculate current question number
    current_question_number = data.index(q) + 1
    
    st.header(f"Question {current_question_number}/{total}: {q['question_text']}")
    answers = q['answers']
    llm_names = list(answers.keys())
    st.subheader("Answers:")
    
    # Check if question was already graded
    is_graded = q['question_id'] in graded_ids
    if is_graded:
        # Get the previous evaluation to preselect
        c.execute("SELECT winning_llm_name FROM evaluation_results WHERE question_id = ?", (q['question_id'],))
        result = c.fetchone()
        index = llm_names.index(result[0]) if result else None
        best = st.radio("Select the best answer:", llm_names, index=index, format_func=lambda x: f"{x}: {answers[x]}")
    else:
        # Don't preselect for ungraded questions
        best = st.radio("Select the best answer:", llm_names, index=None, format_func=lambda x: f"{x}: {answers[x]}")
    if st.button("Select as Best"):
        c.execute(
            "INSERT INTO evaluation_results (question_id, question_text, best_answer_text, winning_llm_name) VALUES (?, ?, ?, ?)",
            (q['question_id'], q['question_text'], answers[best], best)
        )
        conn.commit()
        st.success("Result saved!")
        st.rerun()
    conn.close() 