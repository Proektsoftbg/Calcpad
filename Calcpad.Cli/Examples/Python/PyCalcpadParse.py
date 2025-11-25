# PyCalcpadParse.py

from PyCalcpadWrapper import Parser, Settings
# Initialize the Settings and Parser classes
settings = Settings()
settings.Math.Decimals = 15
parser = Parser()
parser.Settings = settings

# Find the default documents path
from os import path
docsPath = path.expanduser(r"~\Documents")

# read the code from an example file: Continuous beam.txt
print("Parsing Calcpad worksheets with Python + PyCalcpad.")
inputFileName = docsPath + r"\Calcpad\Examples\Mechanics\Structural Analysis\Continuous beam\Continuous beam.txt"
print("Reading the code from the example file:" + inputFileName + "'...")
import io
with io.open(inputFileName,'r',encoding='utf8') as f:
    code = f.read()

# Parse and get the output Html code
print("Parsing and getting the output Html code...")
htmlOutput = parser.Parse(code)

# Apply the Calcpad Html template
templateFile = r"c:\Program Files\Calcpad\doc\template.html"
with io.open(templateFile,'r',encoding='utf8') as f:
    htmlOutput = f.read() + htmlOutput + "</body></html>"  
    
# Save the output to an Html file
print("Saving the output to the Html file...")
outputFileName = path.splitext(inputFileName)[0] + ".html"
with io.open(outputFileName,'w',encoding='utf8') as f:
    f.write(htmlOutput)

# Run the output file
print("Done. Starting the output file: '" + outputFileName + "'...")
if " " in outputFileName:
    outputFileName = '"' + outputFileName + '"'

import os
os.system(outputFileName)
os.system("pause")