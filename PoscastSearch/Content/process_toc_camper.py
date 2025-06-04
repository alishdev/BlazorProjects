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
    main = "<H1>Camp Leaders Book</H1>"
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
      "Chapter 1": "The Camp Experience: Building Blocks for Success",
      "Subtopics": [
        "The Impact of Summer Camp",
        "Building a Strong Sense of Community",
        "Creating Opportunities for Personal Growth",
        "Developing Essential Life Skills"
      ]
    },
    {
      "Chapter 2": "From Camp Counselor to Camp Leader: Developing Leadership Skills",
      "Subtopics": [
        "Staff Training",
        "Leading Activities",
        "Promoting a Culture of Leadership",
        "Developing Responsibility and Initiative"
      ]
    },
    {
      "Chapter 3": "Building Resilience and Adaptability Through Camp Experiences",
      "Subtopics": [
        "Stepping Outside Comfort Zones",
        "Trying New Things",
        "Learning to Adapt",
        "Fostering Resilience"
      ]
    },
    {
      "Chapter 4": "Camp Traditions and Memorable Moments: Crafting a Compelling Narrative",
      "Subtopics": [
        "Campfire Programs and Skits",
        "Wrap-up Ceremonies",
        "Special Events and Traditions",
        "Reflecting on Personal Impact"
      ]
    },
    {
      "Chapter 5": "Community Engagement at Camp: Demonstrating Social Responsibility",
      "Subtopics": [
        "Giving Back Through Volunteering",
        "Supporting Local Charities and Organizations",
        "Engaging Campers in Community Service Projects",
        "Building Relationships with Local Services"
      ]
    },
    {
      "Chapter 6": "Promoting Personal Development and Mental Health",
      "Subtopics": [
        "Recognizing the Importance of Mental Health",
        "Reducing Stress and Anxiety",
        "Providing Support and Encouragement",
        "Building Self-Esteem and Confidence"
      ]
    },
    {
      "Chapter 7": "Navigating Generational Differences",
      "Subtopics": [
        "Challenges",
        "Connecting",
        "Learning Styles",
        "Communication Styles"
      ]
    },
    {
      "Chapter 8": "Communicating Effectively: Building Relationships with Parents and Staff",
      "Subtopics": [
        "Creating a Welcoming Environment",
        "Practicing Active Listening",
        "Delivering Honest Feedback",
        "Understanding Different Communication Styles"
      ]
    },
    {
      "Chapter 9": "Setting Expectations",
      "Subtopics": [
        "Camp Environment",
        "Consistent Expectations",
        "Consistent Enforcement",
        "Clarity"
      ]
    },
    {
      "Chapter 10": "Technology Use",
      "Subtopics": [
        "The value of digital breaks",
        "Benefits to being unplugged",
        "Finding a balance",
        "Tips for staff to be responsible with Technology"
      ]
    },
    {
      "Chapter 11": "Building a great Camp Culture",
      "Subtopics": [
        "Building a business-like atmosphere",
        "Ensuring safety with a friendly-environment",
        "Importance of great hiring",
        "Importance of financial planning"
      ]
    },
    {
      "Chapter 12": "Camp Director's Mentality",
      "Subtopics": [
        "Managing Internal Dialogues",
        "Stress",
        "Building Peer Support",
        "Setting Up a Business"
      ]
    }
  ]
}

# Create the list of topics
topics = create_topic_list(json_data)

# Print or use the topics
for topic in topics:
    print(topic) 