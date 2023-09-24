import os

# Define the input and output directories
input_directory = 'Replace_with_your_input_directory'  # Replace with your input directory ***NOTE: make sure to use "//" when you put yor input directory for example: "C:\\Users\\youruser\Desktop\\mydirectory"***
output_directory = 'Replace_with_your_output_directory'  # Replace with your output directory ***NOTE: make sure to use "//" when you put yor input directory for example: "C:\\Users\\youruser\Desktop\\myoutputdirectory"***

# Create the output directory if it doesn't exist
os.makedirs(output_directory, exist_ok=True)

# Function to split the Markdown file
def split_md_file(input_file, output_directory):
    with open(input_file, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    section = []
    section_count = 0

    for line in lines:
        if line.strip() == '****':
            if section:
                section_count += 1
                output_file = os.path.join(output_directory, f'section_{section_count}.md')
                with open(output_file, 'w', encoding='utf-8') as output_f:
                    output_f.writelines(section)
                section = []
        else:
            section.append(line)

    # Write the last section if it exists
    if section:
        section_count += 1
        output_file = os.path.join(output_directory, f'section_{section_count}.md')
        with open(output_file, 'w', encoding='utf-8') as output_f:
            output_f.writelines(section)

if __name__ == '__main__':
    input_file = os.path.join(input_directory, 'Replace_with_your_input_Markdown_file.md')  # Replace with your input Markdown file for example: "C:\\Users\\youruser\Desktop\\exported_notes_14-09-10_16-14-30.md"
    split_md_file(input_file, output_directory)
