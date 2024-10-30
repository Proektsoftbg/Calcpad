# How to use Calcpad with Python

Since version VM 7.1.0 you can access the Calcpad math engine from Python via the new PyCalcpad.dll API library. It is wrapped on its turn by PyCalcpadWrapper.py that allows you to call all methods entirely in Python language. Because Calcpad is a .NET application, the wrapper relies on the third party library [Python.NET](https://pythonnet.github.io/) that provides the connection between Python and .NET based languages.

## Prerequisites
In order to use Calcpad with Python, you need to install the following prerequisites:
1. Python for Windows: https://www.python.org/downloads/
2. Python.NET by typing the following command in Windows Command Editor: ```pip install pythonnet``` 
3. Install the latest Calcpad: https://calcpad.eu/download/calcpad-VM-setup-en-x64.zip

## Classes and methods
The available classes and methods you can use in PyCalcpad.dll and PyCalcpadWrapper.py are as follows:

```class Calculator:```  
&emsp;&emsp;```Calculator(mathSettings : MathSettings)``` - creates a new Calculator object with predefined math settings.  
&emsp;&emsp;```SetVariable(name : str, value  : float)``` - sets the value of a variable with the specified name.   
&emsp;&emsp;```Run(code : str)``` - calculates a single expression, specified in the 'code' string and returns the complete output containing the original expression, substituted variables and the result.  
&emsp;&emsp;```Eval(code : str)```- the same as above, but returns only the final result. It is also automatically saved to a variable named 'ans', which can be used in further calculations.  

```class Parser:```  
&emsp;&emsp;```Parser()``` - creates a new Parser object with the default settings.  
&emsp;&emsp;```Parse(code : str)``` - parses and calculates an entire Calcpad worksheet, contained in the 'code' string.  
&emsp;&emsp;```Convert(inputFileName : str, outputFileName : str)``` - parses and calculates a Calcpad worksheet, contained in file with path 'inputFileName' and saves the result to 'outputFileName'.  
&emsp;&emsp;Notes:  
&emsp;&emsp;&emsp;&emsp;1.'outputFileName' must be a file path/name with one of the following extensions: 'html', 'pdf' or 'docx'. The program will use the appropriate file format.  
&emsp;&emsp;&emsp;&emsp;2.Alternatively, you can specify only the output format by writing either 'html', 'pdf' or 'docx', without file name. Then, the program will use the input file name by replacing the extension, accordingly.  
&emsp;&emsp;```Settings :  Settings``` - Assigns a settings object of class 'Settings' as described below.  

```class Settings:```  
&emsp;&emsp;```Settings()``` - creates a new 'Settings' object. Default values are assigned to all fields as specified further.  
&emsp;&emsp;```Math : MathSettings``` = MathSettings() - reads/assigns the math settings field, which is an object of class 'MathSettings'.  
&emsp;&emsp;```Plot : PlotSettings``` = PlotSettings() - reads/assigns the plot settings field, which is an object of class 'PlotSettings'.  
&emsp;&emsp;```Units : str``` = 'm' - units that should replace %u in comments. For example, you can specify 'm', 'cm' or 'mm'.  

```class MathSettings:```  
&emsp;&emsp;```MathSettings()``` - creates a new 'MathSettings' object. Default values are assigned to all fields as specified further.  
&emsp;&emsp;```Decimals : int``` = 2 - the number of digits after the decimal point (0-15).  
&emsp;&emsp;```Degrees : TrigUnits``` = TrigUnits.Deg - the default units trigonometric functions (0-2): 0=Degrees, 1=Radians, 2=Grades.  
&emsp;&emsp;```IsComplex : bool``` = False - 'True' enables calculations in complex numbers domain, and 'False' - in real numbers domain only.  
&emsp;&emsp;```Substitute : bool``` = True - switches variable substitution in the output on (True) and off (False).  
&emsp;&emsp;```FormatEquations : bool``` = True - specifies if the equations are formatted in professional or linear styles in the output.  
&emsp;&emsp;```ZeroSmallMatrixElements : bool``` = True - sets if small vector/matrix elements (&lt; 10^-14 times of the max. element magnitude) to be displayed as zeros.  
&emsp;&emsp;```MaxOutputCount : bool``` = 20 - sets the maximum number of elements in a vector or matrix rows/columns to be displayed in the output (5-100).  

```class PlotSettings:```  
&emsp;&emsp;```PlotSettings()``` - creates a new 'PlotSettings' object. Default values are assigned to all fields as specified further.  
&emsp;&emsp;```IsAdaptive : bool``` = True - enables adaptive plotting. It means that the nodes where the function is calculated are not spaced equally but in dependence of the curvature.  
&emsp;&emsp;```VectorGraphics : bool``` = False - specifies whether the plots are generated as vector (SVG) images - 'True', or raster (PNG) - 'False'.  
&emsp;&emsp;```ColorScale : ColorScales``` = ColorScales.Rainbow - sets the current color scale of surface ($map) plots to one of the predefined scales (0-6).  
&emsp;&emsp;```SmoothScale : bool``` = False - if 'True', the scale colors fuse as a smooth gradient, otherwise - uniformly colored isobands are displayed.  
&emsp;&emsp;```Shadows : bool``` = True - specifies if surface plots are displayed with light reflection and shadows ('True') or flat ('False') by using the Blinn-Phong shading model.  
&emsp;&emsp;```LightDirection : LightDirections``` = LightDirections.NorthWest - sets the direction to the light source for the shading (0-7).  

The following enum classes are available for easier input of some restricted values:
```
class TrigUnits(enum.Enum):
	Deg = 0
	Rad = 1
	Grad = 2
```

```
class ColorScales(enum.Enum):
	Transparent = 0
	Gray = 1
	Rainbow = 2
	Terrain = 3
	VioletToYellow = 4
	GreenToYellow = 5
	Blues = 6
```

```
class LightDirections(enum.Enum):
	North = 0
	NorthEast = 1
	East = 2
	SouthEast = 3
	South = 4
	SouthWest = 5
	West = 6
	NorthWest = 7
```
