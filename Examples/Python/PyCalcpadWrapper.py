# PyCalcpadWrapper.py

# Load .NET core from Python.NET
from pythonnet import load
load("coreclr")
import clr

# Load PyCalcpad
import sys, os
programPath = os.environ.get("PROGRAMFILES") + r"\Calcpad"
sys.path.append(programPath)
clr.AddReference(programPath + r"\PyCalcpad.dll")

# Import the specific classes from the PyCalcpad namespace
from System import Type, Activator

# Get the types from the assembly
SettingsType = Type.GetType("PyCalcpad.Settings, PyCalcpad")
MathSettingsType = Type.GetType("PyCalcpad.MathSettings, PyCalcpad")
PlotSettingsType = Type.GetType("PyCalcpad.PlotSettings, PyCalcpad")
CalculatorType = Type.GetType("PyCalcpad.Calculator, PyCalcpad")
ParserType = Type.GetType("PyCalcpad.Parser, PyCalcpad")

import enum
class TrigUnits(enum.Enum):
    Deg = 0
    Rad = 1
    Grad = 2
    
class LightDirections(enum.Enum):
    North = 0
    NorthEast = 1
    East = 2
    SouthEast = 3
    South = 4
    SouthWest = 5
    West = 6
    NorthWest = 7

class ColorScales(enum.Enum):
    Transparent = 0
    Gray = 1
    Rainbow = 2
    Terrain = 3
    VioletToYellow = 4
    GreenToYellow = 5
    Blues = 6

# Define Python classes that wrap the .NET types
class MathSettings:
    def __init__(self, instance=None):
        self._instance = instance or Activator.CreateInstance(MathSettingsType)

    @property
    def Decimals(self):
        return self._instance.Decimals

    @Decimals.setter
    def Decimals(self, value : int):
        self._instance.Decimals = value

    @property
    def Degrees(self):
        return self._instance.Degrees

    @Degrees.setter
    def Degrees(self, value : TrigUnits):
        self._instance.Degrees = value.value        

    @property
    def IsComplex(self):
        return self._instance.IsComplex

    @IsComplex.setter
    def IsComplex(self, value : bool):
        self._instance.IsComplex = value

    @property
    def Substitute(self):
        return self._instance.Substitute

    @Substitute.setter
    def Substitute(self, value : bool):
        self._instance.Substitute = value

    @property
    def FormatEquations(self):
        return self._instance.FormatEquations

    @FormatEquations.setter
    def FormatEquations(self, value : bool):
        self._instance.FormatEquations = value   

    @property
    def ZeroSmallMatrixElements(self):
        return self._instance.ZeroSmallMatrixElements

    @ZeroSmallMatrixElements.setter
    def ZeroSmallMatrixElements(self, value : bool):
        self._instance.ZeroSmallMatrixElements = value

    @property
    def MaxOutputCount(self):
        return self._instance.MaxOutputCount

    @MaxOutputCount.setter
    def MaxOutputCount(self, value : int):
        self._instance.MaxOutputCount = value

class PlotSettings:
    def __init__(self, instance=None):
        self._instance = instance or Activator.CreateInstance(PlotSettingsType)

    @property
    def IsAdaptive(self):
        return self._instance.IsAdaptive

    @IsAdaptive.setter
    def IsAdaptive(self, value: bool):
        self._instance.IsAdaptive = value
        
    @property
    def VectorGraphics(self):
        return self._instance.VectorGraphics

    @VectorGraphics.setter
    def VectorGraphics(self, value : bool):
        self._instance.VectorGraphics = value

    @property
    def ColorScale(self):
        return self._instance.ColorScale

    @ColorScale.setter
    def ColorScale(self, value : ColorScales):
        self._instance.ColorScale = value.value

    @property
    def SmoothScale(self):
        return self._instance.SmoothScale

    @ColorScale.setter
    def SmoothScale(self, value : bool):
        self._instance.SmoothScale = value

    @property
    def Shadows(self):
        return self._instance.Shadows

    @ColorScale.setter
    def Shadows(self, value : bool):
        self._instance.Shadows = value

    @property
    def LightDirection(self):
        return self._instance.LightDirection

    @ColorScale.setter
    def LightDirection(self, value: LightDirections):
        self._instance.LightDirection = value.value

class Settings:
    def __init__(self):
        self._instance = Activator.CreateInstance(SettingsType)

    @property
    def Math(self):
        return MathSettings(self._instance.Math)

    @Math.setter
    def Math(self, value : MathSettings):
        self._instance.Math = value._instance

    @property
    def Plot(self):
        return PlotSettings(self._instance.Plot)

    @Plot.setter
    def Plot(self, value : PlotSettings):
        self._instance.Plot = value._instance

    @property
    def Units(self):
        return self._instance.Units

    @Units.setter
    def Units(self, value : str): 
        self._instance.Units = value

class Calculator:
    def __init__(self, mathSettings : MathSettings):
        self._instance = Activator.CreateInstance(CalculatorType, mathSettings._instance)

    def SetVariable(self, name : str, value : float):
        self._instance.SetVariable(name, value)

    def Run(self, code : str):
        return self._instance.Run(code)

    def Eval(self, code : str):
        return self._instance.Eval(code)

class Parser:
    def __init__(self):
        self._instance = Activator.CreateInstance(ParserType)

    def Parse(self, code : str):
        return self._instance.Parse(code)

    def Convert(self, inputFileName : str, outputFileName : str):
        return self._instance.Convert(inputFileName, outputFileName)

    @property
    def Settings(self):
        return Settings(self._instance.Settings)

    @Settings.setter
    def Settings(self, value : Settings):
        self._instance.Settings = value._instance