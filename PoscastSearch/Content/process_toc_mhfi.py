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
    main = "<H1>FIRE Book</H1>"
    main += "<H2>Table of Contents</H2>"
    main += "<ul>"

    for chapter in json_data["TOC"]:
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
  "TOC": [
    {
      "Chapter 1": "Introduction to Financial Independence (FI)",
      "Subtopics": [
        "Understanding FIRE movement",
        "Misconceptions about FIRE",
        "Personal motivations for pursuing FIRE"
      ]
    },
    {
      "Chapter 2": "Financial Planning and Investment Strategies for FI",
      "Subtopics": [
        "Index fund investing vs. active investing",
        "Real estate investing: myths and strategies",
        "Alternative investments (e.g., cryptocurrency)",
        "Tax optimization strategies",
        "The 4% Rule: applicability and limitations"
      ]
    },
    {
      "Chapter 3": "Career and Education: Balancing Passion and Financial Security",
      "Subtopics": [
        "Choosing a college major",
        "College alternatives: trade schools, coding bootcamps, entrepreneurship",
        "The role of education in achieving financial independence",
        "Developing valuable skills: communication, technical abilities, interpersonal effectiveness",
        "Workplace autonomy and job satisfaction",
        "Side Hustles and multiple sources of income"
      ]
    },
    {
      "Chapter 4": "Mindset and Personal Development on the Path to FI",
      "Subtopics": [
        "Building confidence and self-esteem",
        "Dealing with criticism and negativity",
        "Overcoming fear and embracing uncertainty",
        "Finding purpose beyond traditional work",
        "Building resilience and adapting to change",
        "Forgiving yourself for financial mistakes"
      ]
    },
    {
      "Chapter 5": "The Role of Community in Achieving FI",
      "Subtopics": [
        "Building a supportive network",
        "The power of shared experiences",
        "Finding role models and mentors",
        "Online communities and meetup groups",
        "Camp FI: community building and building lasting relationships"
      ]
    },
    {
      "Chapter 6": "Relationships and FI: Communicating Financial Goals with Loved Ones",
      "Subtopics": [
        "Aligning financial values with a spouse or partner",
        "Open communication about finances in relationships",
        "Navigating differences in financial styles and values",
        "Managing expectations of family"
      ]
    },
    {
      "Chapter 7": "Lifestyle Design: Optimizing Happiness and Well-being on the FI Journey",
      "Subtopics": [
        "Prioritizing experiences over material possessions",
        "Cultivating happiness and well-being alongside financial goals",
        "The unexpected truth about happiness and FI",
        "Avoiding common pitfalls in FI journey"
      ]
    },
    {
      "Chapter 8": "Housing and Transportation: Key Expenses in the FI Lifestyle",
      "Subtopics": [
        "Renting vs. Buying: a financial comparison",
        "Strategies for affordable housing: house hacking, geographic arbitrage",
        "Cars and the FIRE lifestyle: balancing enthusiasm with responsibility",
        "Alternative transportation options"
      ]
    },
    {
      "Chapter 9": "Health and Healthcare in FI",
      "Subtopics": [
        "Health sharing organizations: an alternative to traditional insurance",
        "Direct primary care: a focus on the patient-doctor relationship",
        "Prioritizing physical and mental health",
        "The importance of sleep for performance",
        "Fitness goals and sustainable routines"
      ]
    },
    {
      "Chapter 10": "Life After Achieving FI: Purpose, Passion, and Giving Back",
      "Subtopics": [
        "Making plans after FI",
        "Adjusting to changes in social settings after FI"
      ]
    },
    {
      "Chapter 11": "Raising Financially Savvy Children",
      "Subtopics": [
        "Instilling Financial Values",
        "Teaching About Debt and Credit",
        "Practical Experience with Money",
        "Encouraging Philanthropy"
      ]
    }
  ]
}

# Create the list of topics
topics = create_topic_list(json_data)

# Save topics to a file
with open("topics.txt", "w") as f:
    for topic in topics:
        f.write(f"{topic}\n") 