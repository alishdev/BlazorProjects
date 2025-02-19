import json
import re

def clean_filename(text):
    """Remove special characters and spaces from filename"""
    # Replace spaces and special characters with underscore
    cleaned = re.sub(r'[^a-zA-Z0-9]', '_', text)
    # Remove multiple consecutive underscores
    cleaned = re.sub(r'_+', '_', cleaned)
    # Remove leading/trailing underscores
    cleaned = cleaned.strip('_')
    return cleaned.lower()

def create_topic_list(json_data):
    topics = []
    main = "<H1>College Admissions Book</H1>"
    main += "<H2>Table of Contents</H2>"
    main += "<ul>"

    for chapter in json_data["College Admissions Book - Table of Contents"]:
        chapter_name = list(chapter.values())[0]  # Get chapter name
        main += f"<li><a href='{clean_filename(chapter_name)}.html'>{chapter_name}</a></li>"
        main += "<ul>"
        chapter_file = f"<H2>{chapter_name}</H2>"
        chapter_file += "<ul>"
        subtopics = chapter["Subtopics"]
        
        # Create concatenated strings for each subtopic
        for subtopic in subtopics:
            topic_string = f"{subtopic}"
            topics.append(topic_string)
            chapter_file += f"<li><a href='{clean_filename(subtopic)}.html'>{subtopic}</a></li>"
        chapter_file += "</ul>"

        with open(f"{clean_filename(chapter_name)}.html", "w") as f:
            f.write(chapter_file)
        main += "</ul>"

    main += "</ul>"
    with open("main.html", "w") as f:
        f.write(main) 
    return topics

# Your JSON data
json_data = {
    "College Admissions Book - Table of Contents": [
        {
      "Chapter 1": "Building Your College List",
      "Subtopics": [
        "Understanding School Categories: Reaches, Targets, and Safeties",
        "Admissions by the Numbers vs. Holistic Admissions",
        "The Importance of 'Financial Match' Schools",
        "Factoring in Personal Preferences (Location, Size, Campus Culture)",
        "Leveraging College Websites for In-Depth Research",
        "Identifying Demonstrated Interest (DI) at Different Schools"
      ]
    },
    {
      "Chapter 2": "Maximizing Your Application Profile",
      "Subtopics": [
        "High School Course Selection: Rigor vs. Grades",
        "The Significance of Grade Trends",
        "Crafting a Compelling Activity List",
        "Highlighting Leadership Roles",
        "Building Relationships with Teachers and Counselors",
        "Creating a 'Brag Sheet' or Activity Resume",
        "Preparing Visual Arts Portfolios and Music Auditions",
        "Building a Meaningful Summer Experience",
        "Navigating Early Action vs. Early Decision Strategies",
        "Submitting Competitive Subject Test Scores"
      ]
    },
    {
      "Chapter 3": "Crafting a Standout Essay",
      "Subtopics": [
        "Choosing the Right Essay Prompt",
        "Identifying the 'Aha' Moment",
        "Writing Authentically and Vulnerably",
        "Finding Your Unique Voice",
        "Effective Storytelling Techniques",
        "Common Essay Mistakes to Avoid",
        "Reviewing Essays Effectively",
        "Developing a Strong Conclusion",
        "Addressing Weaknesses in the Essay",
        "Submitting Your Best Writing"
      ]
    },
    {
      "Chapter 4": "Tackling Standardized Tests",
      "Subtopics": [
        "SAT vs. ACT: Choosing the Right Test",
        "Understanding Score Choice and Superscoring Policies",
        "Creating a Strategic Test Prep Plan",
        "Balancing Preparation with Overall Well-being",
        "Free Test Prep Resources and How to Use Them Effectively",
        "The Diminishing Importance of the SAT/ACT Writing Section",
        "How to determine to submit with or without the test",
        "Understanding Different Testing Plans"
      ]
    },
    {
      "Chapter 5": "Understanding the Holistic Admissions Process",
      "Subtopics": [
        "Defining Holistic Admissions vs. Admissions by the Numbers",
        "The Importance of Intangibles (Drive, Resilience, EQ)",
        "What Admission Officers Really Value (Authenticity, Character, Fit)",
        "Environmental Context Dashboard (ECD): What You Need to Know",
        "The Impact of Legacy Status, Athletic Recruitment, and Development Cases",
        "The weight and impact of factors such as grades and test scores",
         "Factors In and out of your control"
      ]
    },
    {
      "Chapter 6": "Financing Your College Education",
      "Subtopics": [
        "Understanding Cost of Attendance (COA)",
        "Calculating Expected Family Contribution (EFC)",
        "Completing the FAFSA and CSS Profile",
        "Managing Parent PLUS Loan Debt",
        "Income-Driven Repayment Plans",
        "Leveraging Net Price Calculators",
        "Evaluating Financial Aid Award Letters",
        "External scholarships from corporations, civic organizations, churches, philanthropist foundations",
        "Parental Debt"
      ]
    },
    {
      "Chapter 7": "Specific Situations and Applicant Groups",
      "Subtopics": [
        "Navigating the Process as a First-Generation Student",
        "Advice for Homeschool Applicants",
        "Applying as an American Expat",
        "Strategies for Student-Athletes",
        "Resources for Students with Disabilities",
        "Understanding Transfer Admissions",
        "Helping a 'C' Student Find College Options",
        "The International Student Admissions Landscape",
        "Second Semester Admissions"
      ]
    },
    {
      "Chapter 8": "Making the Most of College Visits",
      "Subtopics": [
        "Virtual vs. In-Person Visits",
        "The Power of Connecting with Students and Admissions Staff",
        "Campus Tours, Information Sessions, and Customized Visits",
        "Meeting the Staff and Asking the Right Questions",
        "Assessing Campus Life and Fit",
        "Taking the Campus and Taking the School"
      ]
    },
    {
      "Chapter 9": "Decoding College Rankings and Perceptions",
      "Subtopics": [
        "The Truth Behind College Rankings and How to use them responsibly",
        "How Colleges 'Game' the System",
        "The Value of College Reviews",
        "Dangers of the thought of the idea that an elite college is important",
        "Leveraging Reviews to Formulate Questions"
      ]
    },
    {
      "Chapter 10": "When Things Don't Go According to Plan",
      "Subtopics": [
        "What to Do If You Don't Get In Anywhere",
        "Navigating Deferrals and Waitlists",
        "Considering Second-Semester Admissions",
        "Coping with Rejection and Maintaining Perspective",
        "The impact of test scores on admissions decisions",
        "Rescinded Offers and How to Avoid Them",
        "Options are limited for average students"
      ]
    },
    {
      "Chapter 11": "Parental Involvement: Finding the Right Balance",
      "Subtopics": [
        "Providing Support Without Overstepping",
        "Communicating Expectations and Financial Boundaries",
        "Managing Stress and Avoiding Parental Ego",
        "The impact of rankings and prestige",
        "Trust and Letting Go"
      ]
    },
    {
     "Chapter 12": "Thriving in College and Beyond",
      "Subtopics":[
         "Student loan debt and financial aid",
         "Thriving on college with a plan",
         "Finding a major",
         "Tips to relieve stress during the application process",
          "Utilizing a college's career center and resources"
         ]
    }
    ]
}

# Create the list of topics
topics = create_topic_list(json_data)

# Print or use the topics
for topic in topics:
    print(topic) 