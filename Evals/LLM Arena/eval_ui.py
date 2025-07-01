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
    # Find first ungraded question
    ungraded = [q for q in data if q['question_id'] not in graded_ids]
    total = len(data)
    graded = total - len(ungraded)
    st.title("LLM Arena: Human Evaluation")
    st.write(f"Graded {graded} / {total}")
    if not ungraded:
        st.success("Completed!")
        return
    q = ungraded[0]
    st.header(f"Question: {q['question_text']}")
    answers = q['answers']
    llm_names = list(answers.keys())
    st.subheader("Answers:")
    best = st.radio("Select the best answer:", llm_names, format_func=lambda x: f"{x}: {answers[x]}")
    if st.button("Select as Best"):
        c.execute(
            "INSERT INTO evaluation_results (question_id, question_text, best_answer_text, winning_llm_name) VALUES (?, ?, ?, ?)",
            (q['question_id'], q['question_text'], answers[best], best)
        )
        conn.commit()
        st.success("Result saved!")
        st.rerun()
    conn.close() 