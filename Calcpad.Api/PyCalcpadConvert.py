# PyCalcpadConvert.py

from PyCalcpadWrapper import Parser, Settings
# Initialize the Settings and Parser classes
settings = Settings()
settings.Math.Decimals = 15
parser = Parser()
parser.Settings = settings
# Find the default documents path
from os import path
docsPath = path.expanduser(r"~\Documents")
# Convert the code from an example file: Continuous beam.txt to a Word document
inputFileName = docsPath + r"\Calcpad\Examples\Mechanics\Structural Analysis\Continuous beam\Continuous beam.txt"
print("Convert Calcpad worksheets with Python + PyCalcpad.")
print("Input file: '" + inputFileName + "'.")
ext = input("Enter the extension of the output file (docx, html, pdf): ")
print("Converting the example file: '" + inputFileName + "' to '" + ext + "'...")
parser.Convert(inputFileName, ext)

# Run the output docx file
outputFileName = path.splitext(inputFileName)[0] + "." + ext
print("Done. Starting the output file: '" + outputFileName + "'...")
if " " in outputFileName:
    outputFileName = '"' + outputFileName + '"'

import os
os.system(outputFileName)
os.system("pause")