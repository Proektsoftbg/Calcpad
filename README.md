# Calcpad Readme

Project Website: [https://calcpad.eu](https://calcpad.eu)

Calcpad is free software for mathematical and engineering calculations. It represents a flexible and modern programmable calculator with Html report generator. It is simple and easy to use, but it also includes many advanced features:

* real and complex numbers;
* units of measurement (SI, Imperial and USCS);
* custom variables;
* built-in library with common math functions;
* custom functions of multiple parameters f(x; y; z; ...);
* powerful numerical methods for root and extremum finding, numerical integration and differentiation;
* finite sum, product and iteration procedures;
* program flow control with conditions and loops;
* "titles" and 'text' comments in quotes; 
* support for Html and CSS in comments for rich formatting;
* function plotting, images, tables, parametric SVG drawings, etc.;
* automatic generation of Html forms for data input;
* professional looking Html reports for viewing printing;
* variable substitution and smart rounding of numbers; 
* output visibility control and content folding;
* support for plain text (*.txt, *.cpd) and binary (*.cpdz) file formats.

This software is developed using the C# programming language and the latest computer technologies. It automatically parses the input, substitutes the variables, calculates the expressions and displays the output. All results are sent to a professional looking Html report for viewing and printing. Acknowledgments: The new and beautiful icons are created using [https://icons8.com](https://icons8.com). The pdf export was made possible thanks to the [wkhtmltopdf.org](https://wkhtmltopdf.org/) project.

![Sample](https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Sample.png?raw=true)

## Fields of application
This software is suitable for engineers and other professionals that need to perform repetitive calculations and present them in official documentation such as calculation notes. They can automate this task efficiently by creating powerful and reliable Calcpad worksheets. It can also help teachers to prepare calculation examples, papers, manuals, books etc. Students can use it to solve various problems, prepare homeworks, phd theses etc.

## Installation
The installation is performed by the automated setup program [calcpad-setup-en-x64.exe](https://github.com/Proektsoftbg/Calcpad/blob/main/Setup/calcpad-setup-en-x64.exe). Follow the instruction of the setup wizard. The software requires a 64 bit computer with Windows 10 and [Microsoft .NET 6.0](https://download.visualstudio.microsoft.com/download/pr/a865ccae-2219-4184-bcd6-0178dc580589/ba452d37e8396b7a49a9adc0e1a07e87/windowsdesktop-runtime-6.0.0-win-x64.exe).
You can also use Calcpad directly in the browser from our website: [https://calcpad.eu/Ide](https://calcpad.eu/Ide)

## Licensing and terms of use
This software is free for both commercial and non-commercial use. It is distributed under the MIT license:

Copyright © 2021 PROEKTSOFT EOOD®

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Any scripts, developed with Calcpad are property of the respective authors. They can be used without additional limitations except those appointed by the authors themselves.

## How it works
The software is quick and easy to use. Just follow these simple steps:

1. **Enter** text and formulas into the "**Code**" box on the left.
2. Click <img alt="Play" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Play.png"> to calculate. Results will appear in the "**Output**" box on the right as a professionally formatted Html **report**.
3. Click <img alt="PrintPreview" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/PrintPreview.png"> to **print** or <img alt="Copy" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Copy.png"> to **copy** the output.
You can also **export** it to Html <img alt="Html" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Save.png">, PDF <img alt="PDF" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Pdf.png"> or MS Word <img alt="Word" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Word.png"> document.

## The language

Calcpad uses a simple programming language that includes the following elements:
* Real numbers: digits "0" - "9" and decimal point ".";
* Complex numbers: re ± imi (e.g. 3 - 2i);
* Variables:  
&emsp;&emsp;- Latin letters: "a" - "z", "A" - "Z";  
&emsp;&emsp;- Greek letters: "α" - "ω", "Α" - "Ω";  
&emsp;&emsp;- digits: "0" - "9";  
&emsp;&emsp;- comma: ",";  
&emsp;&emsp;- prime symbols: " ′ ", " ″ ", " ‴ ", " ⁗ ";  
&emsp;&emsp;- special symbols: " ø ", "Ø", " ° ", "∡";
&emsp;&emsp;- "\_" for subscript;  
A variable name must start with a letter. Names are case sensitive.  
* Operators:  
&emsp;&emsp;"!" - factorial;  
&emsp;&emsp;"^" - exponent;  
&emsp;&emsp;"/" - division;  
&emsp;&emsp;"÷" - force division bar;  
&emsp;&emsp;"\" - division;  
&emsp;&emsp;"%" - reminder;  
&emsp;&emsp;"\*" - multiplication;  
&emsp;&emsp;"-" - minus;  
&emsp;&emsp;"+" - plus;  
&emsp;&emsp;"≡" - equal to;  
&emsp;&emsp;"≠" - not equal to;  
&emsp;&emsp;"<" - less than;  
&emsp;&emsp;">" - greater than;  
&emsp;&emsp;"≤" - less or equal;  
&emsp;&emsp;"≥" - greater or equal;  
&emsp;&emsp;"=" - assignment;  
* Custom functions of type f (x; y; z; ... );  
* Built-in functions:  
&emsp;&emsp;abs(x) - absolute value/magnitude;  
&emsp;&emsp;sin(x)  - sine;  
&emsp;&emsp;cos(x)  - cosine;  
&emsp;&emsp;tan(x)  - tangent;  
&emsp;&emsp;csc(x)  - cosecant;  
&emsp;&emsp;sec(x)  - secant;  
&emsp;&emsp;cot(x)  - cotangent;  
&emsp;&emsp;sinh(x) - hyperbolic sine;  
&emsp;&emsp;cosh(x) - hyperbolic cosine;  
&emsp;&emsp;tanh(x) - hyperbolic tangent;  
&emsp;&emsp;csch(x) - hyperbolic cosecant;  
&emsp;&emsp;sech(x) - hyperbolic secant;  
&emsp;&emsp;coth(x) - hyperbolic cotangent;  
&emsp;&emsp;asin(x) - inverse sine;  
&emsp;&emsp;acos(x) - inverse cosine;  
&emsp;&emsp;atan(x) - inverse tangent;  
&emsp;&emsp;atan2(x; y) - the angle whose tangent is the quotient of y and x;  
&emsp;&emsp;acsc(x) - inverse cosecant;  
&emsp;&emsp;asec(x) - inverse secant;  
&emsp;&emsp;acot(x) - inverse cotangent;  
&emsp;&emsp;asinh(x) - inverse hyperbolic sine;  
&emsp;&emsp;acosh(x) - inverse hyperbolic cosine;  
&emsp;&emsp;atanh(x) - inverse hyperbolic tangent;  
&emsp;&emsp;acsch(x) - inverse hyperbolic cosecant;  
&emsp;&emsp;asech(x) - inverse hyperbolic secant;  
&emsp;&emsp;acoth(x) - inverse hyperbolic cotangent;  
&emsp;&emsp;log(x) - decimal logarithm;  
&emsp;&emsp;ln(x) - natural logarithm;  
&emsp;&emsp;log2(x) - binary logarithm;  
&emsp;&emsp;sqr(x) / sqrt(x) - square root;  
&emsp;&emsp;cbrt (x) - cubic root;  
&emsp;&emsp;root(x; n) - n-th root;  
&emsp;&emsp;round(x) - round to the nearest integer;  
&emsp;&emsp;floor(x) - round to the lower integer;  
&emsp;&emsp;ceiling(x) - round to the greater integer;  
&emsp;&emsp;trunc(x) - round to the nearest integer towards zero;  
&emsp;&emsp;re(x) - the real part of a complex number;  
&emsp;&emsp;im(x) - the imaginary part of a complex number;  
&emsp;&emsp;phase(x) - the phase of a complex number;  
&emsp;&emsp;random(x) - random number between 0 and x;  
&emsp;&emsp;min(x; y; z...) - minimum of multiple values;  
&emsp;&emsp;max(x; y; z...) - maximum of multiple values;  
&emsp;&emsp;sum(x; y; z...) - sum of multiple values = x + y + z...;  
&emsp;&emsp;sumsq(x; y; z...) - sum of squares = x² + y² + z²...;  
&emsp;&emsp;srss(x; y; z...) - square root of sum of squares = sqrt(x² + y² + z²...);  
&emsp;&emsp;average(x; y; z...) - average of multiple values = (x + y + z...)/n;  
&emsp;&emsp;product(x; y; z...) - product of multiple values = x·y·z...;  
&emsp;&emsp;mean(x; y; z...) - geometric mean = n-th root(x·y·z...);  
&emsp;&emsp;if(*cond*; *value-if-true*; *value-if-false*) - conditional evaluation;   
&emsp;&emsp;switch(*cond1*; *value1*; *cond2*; *value2*; … ; *default*) - selective evaluation;  
&emsp;&emsp;take(n; a; b; c...) - returns the n-th element from the list;  
&emsp;&emsp;line(x; a; b; c...) - linear interpolation;  
&emsp;&emsp;spline(x; a; b; c...) - Hermite spline interpolation;  
* Comments: "Title" or 'text' in double or single quotes, respectively. HTML, CSS, JS and SVG are allowed.  
* Graphing and plotting:  
&emsp;&emsp;$Plot { f(x) @ x = a : b } - simple plot;  
&emsp;&emsp;$Plot { x(t) | y(t) @ t = a : b } - parametric;  
&emsp;&emsp;$Plot { f1(x) & f2(x) & ... @ x = a : b } - multiple;  
&emsp;&emsp;$Plot { x1(t) | y1(t) & x2(t) | y2(t) & ... @ x = a : b } - multiple parametric;  
&emsp;&emsp;$Map { f(x; y) @ x = a : b & y = c : d }  - 2D color map of a 3D surface;  
&emsp;&emsp;PlotHeight - height of plot area in pixels;  
&emsp;&emsp;PlotWidth - width of plot area in pixels;  
* Iterative and numerical methods:  
&emsp;&emsp;$Root { f(x) = const @ x = a : b } - root finding for f(x) = const;  
&emsp;&emsp;$Root { f(x) @ x = a : b } - root finding for f(x) = 0;  
&emsp;&emsp;$Find { f(x) @ x = a : b } similar to above, but x is not required to be a precise solution;  
&emsp;&emsp;$Sup { f(x) @ x = a : b } - local maximum of a function;  
&emsp;&emsp;$Inf { f(x) @ x = a : b } - local minimum of a function;  
&emsp;&emsp;$Area { f(x) @ x = a : b } - numerical integration;  
&emsp;&emsp;$Slope { f(x) @ x = a } - numerical differentiation;  
&emsp;&emsp;$Sum { f(k) @ k = a : b } - iterative sum;  
&emsp;&emsp;$Product { f(k) @ k = a : b } - iterative product;  
&emsp;&emsp;$Repeat { f(k) @ k = a : b } - general inline iterative procedure;  
&emsp;&emsp;Precision - relative precision for numerical methods \[10-2; 10-16\] (default is 10-12)  
* Program flow control:  
&emsp;&emsp;Simple:  
&emsp;&emsp;&emsp;&emsp;#if *condition*  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;&emsp;&emsp;#end if  
&emsp;&emsp;Alternative:  
&emsp;&emsp;&emsp;&emsp;#if *condition*  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;&emsp;&emsp;#else  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Some other code*  
&emsp;&emsp;&emsp;&emsp;#end if  
&emsp;&emsp;Complete:  
&emsp;&emsp;&emsp;&emsp;#if c*ondition1*  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;&emsp;&emsp;#else if *condition2*  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;&emsp;&emsp;#else  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Some other code*  
&emsp;&emsp;&emsp;&emsp;#end if  
You can add or omit as many "#else if's" as needed. Only one "#else" is allowed. You can omit this too.  
* Iteration blocks:  
&emsp;&emsp;Simple:  
&emsp;&emsp;&emsp;&emsp;#repeat *number of repetitions*  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;&emsp;&emsp;#loop  
&emsp;&emsp;With conditional break:  
&emsp;&emsp;&emsp;&emsp;#repeat *number of repetitions*  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;#if *condition*  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;#break  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;#end if  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;*Some more code*  
&emsp;&emsp;&emsp;&emsp;#loop  
* Output control:  
&emsp;&emsp;#hide - hide the report contents;  
&emsp;&emsp;#show - always show the contents (default);  
&emsp;&emsp;#pre - show the next contents only before calculations;  
&emsp;&emsp;#post - show the next contents only after calculations;  
&emsp;&emsp;#val - show only the final result, without the equation;  
&emsp;&emsp;#equ - show complete equations and results (default);  
&emsp;&emsp;Each of the above commands is effective after the current line until the end of the report or another command that overwrites it.  
* Units for trigonometric functions: #deg - degrees, #rad - radians;  
* Separator for target units: |;  
* Metric units (SI and compatible):  
&emsp;&emsp;Mass: g, hg, kg, t, kt, Mt, Gt, dg, cg, mg, μg, Da, u;  
&emsp;&emsp;Length: m, km, dm, cm, mm, μm, nm, pm, AU, ly;  
&emsp;&emsp;Time: s, ms, μs, ns, ps, min, h, d;  
&emsp;&emsp;Frequency: Hz, kHz, MHz, GHz, THz, mHz, μHz, nHz, pHz, rpm;	 
&emsp;&emsp;Velocity: kmh;  
&emsp;&emsp;Electric current: A, kA, MA, GA, TA, mA, μA, nA, pA;  
&emsp;&emsp;Temperature: °C, Δ°C, K;  
&emsp;&emsp;Amount of substance: mol;  
&emsp;&emsp;Luminous intensity: cd;  
&emsp;&emsp;Area: a, daa, ha;  
&emsp;&emsp;Volume: L, mL, cL, dL, hL;  
&emsp;&emsp;Force: dyn N, daN, hN, kN, MN, GN, TN, kgf, tf;  
&emsp;&emsp;Moment: Nm, kNm;  
&emsp;&emsp;Pressure: Pa, daPa, hPa, kPa, MPa, GPa, TPa, dPa, cPa, mPa, μPa, nPa, pPa,  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;bar, mbar, μbar, atm, at, Torr, mmHg;  
&emsp;&emsp;Energy work: J, kJ, MJ, GJ, TJ, mJ, μJ, nJ, pJ, Wh, kWh, MWh, GWh, TWh,  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;cal, kcal, erg, eV, keV, MeV, GeV, TeV, PeV, EeV;  
&emsp;&emsp;Power: W, kW, MW, GW, TW, mW, μW, nW, pW, hpM, ks;  
&emsp;&emsp;Electric charge: C, kC, MC, GC, TC, mC, μC, nC, pC, Ah, mAh;  
&emsp;&emsp;Potential: V, kV, MV, GV, TV, mV, μV, nV, pV;  
&emsp;&emsp;Capacitance: F, kF, MF, GF, TF, mF, μF, nF, pF;  
&emsp;&emsp;Resistance: Ω, kΩ, MΩ, GΩ, TΩ, mΩ, μΩ, nΩ, pΩ;  
&emsp;&emsp;Conductance: S, kS, MS, GS, TS, mS, μS, nS, pS;  
&emsp;&emsp;Magnetic flux: Wb , kWb, MWb, GWb, TWb, mWb, μWb, nWb, pWb;  
&emsp;&emsp;Magnetic flux density: T, kT, MT, GT, TT, mT, μT, nT, pT;  
&emsp;&emsp;Inductance: H, kH, MH, GH, TH, mH, μH, nH, pH;  
&emsp;&emsp;Luminous flux: lm;  
&emsp;&emsp;Illuminance: lx;  
&emsp;&emsp;Radioactivity: Bq, kBq, MBq, GBq, TBq, mBq, μBq, nBq, pBq, Ci, Rd;  
&emsp;&emsp;Absorbed dose: Gy, kGy, MGy, GGy, TGy, mGy, μGy, nGy, pGy;  
&emsp;&emsp;Equivalent dose: Sv, kSv, MSv, GSv, TSv, mSv, μSv, nSv, pSv;  
&emsp;&emsp;Catalytic activity: kat;  
* Non-metric units (Imperial/US):  
&emsp;&emsp;Mass: gr, dr, oz, lb, kip , st, qr, cwt, cwt_UK, cwt_US, ton, ton_UK, ton_US, slug;  
&emsp;&emsp;Length: th, in, ft, yd, ch, fur, mi, ftm, cable, nmi, li, rod, pole, perch, lea;  
&emsp;&emsp;Speed: mph;  
&emsp;&emsp;Temperature: °F, Δ°F, °R;  
&emsp;&emsp;Area: rood, ac;  
&emsp;&emsp;Volume (fluid): fl_oz, gi, pt, qt, gal, bbl, (dry) bu;  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;fl_oz_UK, gi_UK, pt_UK, qt_UK, gal_UK, bbl_UK, (dry) bu_UK;  
&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;fl_oz_US, gi_US, pt_US, qt_US, gal_US, bbl_US, (dry) bu_US;  
&emsp;&emsp;Force: ozf, lbf, kipf, tonf, pdl;  
&emsp;&emsp;Pressure: osi, osf psi, psf, ksi, ksf, tsi, tsf, inHg;  
&emsp;&emsp;Energy/work: BTU, therm, therm_UK, therm_US, quad;  
&emsp;&emsp;Power: hp, hpE, hpS.
