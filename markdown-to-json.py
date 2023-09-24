import os
import json
import hashlib

# Define the input and output directories
input_directory = 'Replace_with_your_input_directory'  # Replace with your input directory ***NOTE: make sure to use "//" when you put yor input directory for example: "C:\\Users\\youruser\Desktop\\mydirectory"***
output_directory = 'Replace_with_your_output_directory'  # Replace with your output directory ***NOTE: make sure to use "//" when you put yor input directory for example: "C:\\Users\\youruser\Desktop\\myoutputdirectory"***

# Create the output directory if it doesn't exist
os.makedirs(output_directory, exist_ok=True)

# Function to convert Markdown to JSON
def markdown_to_json(input_file):
    with open(input_file, 'r', encoding='utf-8') as f:
        content = f.read()

    # Create a unique ID for the content using its MD5 hash
    content_id = hashlib.md5(content.encode('utf-8')).hexdigest()

    # Replace newlines with spaces in the content for JSON formatting
    content = content.replace('\n', ' ')

    # Create a JSON object
    json_obj = {
        "id": content_id,
        "content": content,
        "creationDate": "2023-09-14T17:28:08.006Z",
        "lastModified": "2023-09-14T17:28:08.006Z"
    }

    return json_obj

# Iterate through the split Markdown files and convert to JSON
json_list = []
for filename in os.listdir(input_directory):
    if filename.endswith('.md'):
        input_file = os.path.join(input_directory, filename)
        json_obj = markdown_to_json(input_file)
        json_list.append(json_obj)

# Write the JSON to an output file
output_file = os.path.join(output_directory, 'output.json')
with open(output_file, 'w', encoding='utf-8') as json_file:
    json.dump(json_list, json_file, ensure_ascii=False, indent=2)

print(f"Conversion complete. JSON file saved to {output_file}")
