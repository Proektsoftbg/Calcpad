# Calcpad Readme  
  
Project Website: [https://calcpad.eu](https://calcpad.eu)  
  
Calcpad is free software for mathematical and engineering calculations. It represents a flexible and modern programmable calculator with Html report generator. It is simple and easy to use, but it also includes many advanced features:  
  
* real and complex numbers (rectangular and polar-phasor formats);
* units of measurement (SI, Imperial and USCS);
* vectors and matrices: rectangular, symmetric, column, diagonal, upper/lower triangular;
* custom variables and units;
* built-in library with common math functions;
* vectors and matrix functions:
  * data functions: search, lookup, sort, count, etc.;
  * aggregate functions: min, max, sum, sumsq, srss, average, product (geometric) mean, etc.;
  * math functions: norm, condition, determinant, rank, trace, transpose, adjugate and cofactor, inverse, factorization (cholesky, ldlt, lu, qr and svd), eigenvalues/vectors and linear systems of equations;
* custom functions of multiple parameters f(x; y; z; …);
* powerful numerical methods for root and extremum finding, numerical integration and differentiation;
* finite sum, product and iteration procedures, Fourier series and FFT;
* modules, macros and string variables;
* reading and writing data from/to text, CSV and Excel files;
* program flow control with conditions and loops;
* "titles" and 'text' comments in quotes;
* support for Html and CSS in comments for rich formatting;
* function plotting, images, tables, parametric SVG drawings, etc.;
* automatic generation of Html forms for data input;
* professional looking Html reports for viewing and printing;
* export to Word (\*.docx) and PDF documents;
* variable substitution and smart rounding of numbers;
* output visibility control and content folding;
* support for plain text (\*.txt, \*.cpd) and binary (\*.cpdz) file formats.
  
This software is developed using the C# programming language and the latest computer technologies. It automatically parses the input, substitutes the variables, calculates the expressions and displays the output. All results are sent to a professional looking Html report for viewing and printing.
  
![Sample](https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Sample.png?raw=true)  
  
## Fields of application  
This software is suitable for engineers and other professionals that need to perform repetitive calculations and present them in official documentation such as calculation notes. They can automate this task efficiently by creating powerful and reliable Calcpad worksheets. It can also help teachers to prepare calculation examples, papers, manuals, books etc. Students can use it to solve various problems, prepare homeworks, theses etc.  
  
## Installation  
The installation is performed by the automated setup program [calcpad-VM-setup-en-x64.exe](https://github.com/Proektsoftbg/CalcpadVM/blob/main/Setup/calcpad-VM-setup-en-x64.exe). Follow the instruction of the setup wizard. The software requires a 64 bit computer with Windows 10/11 and [Microsoft .NET Desktop Runtime 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).  
You can also use Calcpad directly in the browser from our website: [https://calcpad.eu/Ide](https://calcpad.eu/Ide)  
  
## Licensing and terms of use  
This software is free for both commercial and non-commercial use. It is distributed under the MIT license:  
  
Copyright © 2025 PROEKTSOFT EOOD®  
  
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:  
  
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.  
  
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.  
  
Any scripts, developed with Calcpad are property of the respective authors. They can be used without additional limitations except those appointed by the authors themselves.  

### Acknowledgments  
This project uses some additional third party components, software and design. They are re-distributed free of charge, under the license conditions, provided by the respective authors.  
1. The new and beautiful icons are created using [icons8.com](https://icons8.com/).  
2. The pdf export was made possible thanks to the [wkhtmltopdf.org](https://wkhtmltopdf.org/) project.  
3. Some symbols are displayed, using the Jost* font family by [indestructible type*](https://indestructibletype.com/), under the [SIL open font license](https://scripts.sil.org/cms/scripts/page.php?item_id=OFL_web).
Square brackets are slightly modified to suit the application needs.  
  
## How it works  
The software is quick and easy to use. Just follow these simple steps:  
  
1. **Enter** text and formulas into the "**Code**" box on the left.  
2. Press **F5** or click <img alt="Play" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Play.png"> to calculate. Results will appear in the "**Output**" box on the right as a professionally formatted Html **report**.  
3. Click <img alt="PrintPreview" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/PrintPreview.png"> to **print** or <img alt="Copy" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Copy.png"> to **copy** the output.  
You can also **export** it to **Html** <img alt="Html" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Save.png">, **PDF** <img alt="PDF" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Pdf.png"> or **MS Word** <img alt="Word" height="24" src="https://github.com/Proektsoftbg/Calcpad/blob/main/Help/Images/Word.png"> document.  
  
## The language  
  
Calcpad uses a simple programming language that includes the following elements:  
* Real numbers: digits 0 - 9 and decimal point ".";  
* Complex numbers: re ± imi (e.g. 3 - 2i);  
* Vectors: [v₁; v₂; v₃; …; vₙ];  
* Matrices: [M₁₁; M₁₂; … ; M₁ₙ | M₂₁; M₂₂; … ; M₂ₙ | … | Mₘ₁; Mₘ₂; … ; Mₘₙ]  
* Variables:  
&emsp;- all Unicode letters;  
&emsp;- digits: 0 - 9;  
&emsp;- comma: " , ";  
&emsp;- special symbols: ′ , ″ , ‴ , ⁗ , ‾ , ø , Ø , ° , ∡ ;  
&emsp;- superscripts: ⁰ , ¹ , ² , ³ , ⁴ , ⁵ , ⁶ , ⁷ , ⁸ , ⁹ , ⁿ , ⁺ , ⁻ ;  
&emsp;- subscripts: ₀ , ₁ , ₂ , ₃ , ₄ , ₅ , ₆ , ₇ , ₈ , ₉ , ₊ , ₋ , ₌ , ₍ , ₎;  
&emsp;- " _ " for subscript;  
Any variable name must start with a letter. Names are case sensitive.  
* Operators:  
&emsp;"**!**" - factorial;  
&emsp;"**^**" - exponent;  
&emsp;"**/**" - division;  
&emsp;"**÷**" - force division bar in inline mode and slash in pro mode (//);  
&emsp;"**\\**" - integer division;  
&emsp;"**⦼**" - modulo (remainder);  
&emsp;"**\***" - multiplication;  
&emsp;"**-**" - minus;  
&emsp;"**+**" - plus;  
&emsp;"**≡**" - equal to;  
&emsp;"**≠**" - not equal to;  
&emsp;"**<**" - less than;  
&emsp;"**>**" - greater than;  
&emsp;"**≤**" - less or equal;  
&emsp;"**≥**" - greater or equal;  
&emsp;"**∧**" - logical "AND";   
&emsp;"**∨**" - logical "OR";   
&emsp;"**⊕**" - logical "XOR";   
&emsp;"**∠**" - phasor A∠φ (<<);   
&emsp;"**=**" - assignment;  
* Custom functions of type f (x; y; z; ... );  
* Built-in functions:  
&emsp;Trigonometric:  
&emsp;&emsp;**sin**(x)  - sine;  
&emsp;&emsp;**cos**(x)  - cosine;  
&emsp;&emsp;**tan**(x)  - tangent;  
&emsp;&emsp;**csc**(x)  - cosecant;  
&emsp;&emsp;**sec**(x)  - secant;  
&emsp;&emsp;**cot**(x)  - cotangent;  
&emsp;Hyperbolic:  
&emsp;&emsp;**sinh**(x) - hyperbolic sine;  
&emsp;&emsp;**cosh**(x) - hyperbolic cosine;  
&emsp;&emsp;**tanh**(x) - hyperbolic tangent;  
&emsp;&emsp;**csch**(x) - hyperbolic cosecant;  
&emsp;&emsp;**sech**(x) - hyperbolic secant;  
&emsp;&emsp;**coth**(x) - hyperbolic cotangent;  
&emsp;Inverse trigonometric:  
&emsp;&emsp;**asin**(x) - inverse sine;  
&emsp;&emsp;**acos**(x) - inverse cosine;  
&emsp;&emsp;**atan**(x) - inverse tangent;  
&emsp;&emsp;**atan2**(x; y) - the angle whose tangent is the quotient of y and x;  
&emsp;&emsp;**acsc**(x) - inverse cosecant;  
&emsp;&emsp;**asec**(x) - inverse secant;  
&emsp;&emsp;**acot**(x) - inverse cotangent;  
&emsp;Inverse hyperbolic:  
&emsp;&emsp;**asinh**(x) - inverse hyperbolic sine;  
&emsp;&emsp;**acosh**(x) - inverse hyperbolic cosine;  
&emsp;&emsp;**atanh**(x) - inverse hyperbolic tangent;  
&emsp;&emsp;**acsch**(x) - inverse hyperbolic cosecant;  
&emsp;&emsp;**asech**(x) - inverse hyperbolic secant;  
&emsp;&emsp;**acoth**(x) - inverse hyperbolic cotangent;  
&emsp;Logarithmic, exponential and roots:  
&emsp;&emsp;**log**(x)   - decimal logarithm;  
&emsp;&emsp;**ln**(x)    - natural logarithm;  
&emsp;&emsp;**log_2**(x) - binary logarithm;  
&emsp;&emsp;**exp**(x)   - natural exponent;  
&emsp;&emsp;**sqr**(x) / sqrt(x) - square root;  
&emsp;&emsp;**cbrt**(x) - cubic root;  
&emsp;&emsp;**root**(x; n) - n-th root;  
&emsp;Rounding:  
&emsp;&emsp;**round**(x) - round to the nearest integer;  
&emsp;&emsp;**floor**(x) - round to the smaller integer (towards -∞);  
&emsp;&emsp;**ceiling**(x) - round to the greater integer (towards +∞);  
&emsp;&emsp;**trunc**(x) - round to the smaller integer (towards zero);  
&emsp;Integer:  
&emsp;&emsp;**mod**(x; y) - the remainder of an integer division;  
&emsp;&emsp;**gcd**(x; y; z...) - the greatest common divisor of several integers;  
&emsp;&emsp;**lcm**(x; y; z...) - the least common multiple of several integers;  
&emsp;Complex:  
&emsp;&emsp;**re**(z)    - the real part of a complex number;  
&emsp;&emsp;**im**(z)    - the imaginary part of a complex number;  
&emsp;&emsp;**abs**(z)   - absolute value/magnitude;  
&emsp;&emsp;**phase**(z) - the phase of a complex number;  
&emsp;&emsp;**conj**(z)  - the conjugate of a complex number;  
&emsp;Aggregate and interpolation:  
&emsp;&emsp;**min**(x; y; z...) - minimum of multiple values;  
&emsp;&emsp;**max**(x; y; z...) - maximum of multiple values;  
&emsp;&emsp;**sum**(x; y; z...) - sum of multiple values = x + y + z...;  
&emsp;&emsp;**sumsq**(x; y; z...) - sum of squares = x² + y² + z²...;  
&emsp;&emsp;**srss**(x; y; z...) - square root of sum of squares = sqrt(x² + y² + z²...);  
&emsp;&emsp;**average**(x; y; z...) - average of multiple values = (x + y + z...)/n;  
&emsp;&emsp;**product**(x; y; z...) - product of multiple values = x·y·z...;  
&emsp;&emsp;**mean**(x; y; z...) - geometric mean = n-th root(x·y·z...);  
&emsp;&emsp;**take**(n; a; b; c...) - returns the n-th element from the list;  
&emsp;&emsp;**line**(x; a; b; c...) - linear interpolation;  
&emsp;&emsp;**spline**(x; a; b; c...) - Hermite spline interpolation;  
&emsp;Conditional and logical:  
&emsp;&emsp;**if**(*cond*; *value-if-true*; *value-if-false*) - conditional evaluation;  
&emsp;&emsp;**switch**(*cond1*; *value1*; *cond2*; *value2*; … ; *default*) - selective evaluation;  
&emsp;&emsp;**not**(x) - logical "NOT";  
&emsp;&emsp;**and**(x; y; z...) - logical "AND";  
&emsp;&emsp;**or**(x; y; z...) - logical "OR";  
&emsp;&emsp;**xor**(x; y; z...) - logical "XOR";  
&emsp;Other:  
&emsp;&emsp;**sign**(x) - the sign of a number;  
&emsp;&emsp;**random**(x) - random number between 0 and x;  
&emsp;&emsp;**getunits**(x) - gets the units of x without the value. Returns 1 if x is unitless;  
&emsp;&emsp;**setunits**(x; u) - sets the units u to x where x can be scalar, vector or matrix;  
&emsp;&emsp;**clrunits**(x) - clears the units from a scalar, vector or matrix x;  
&emsp;&emsp;**hp**(x) - converts x to its high performance (hp) equivalent type;  
&emsp;&emsp;**ishp**(x) - checks if the type of x is a high-performance (hp) vector or matrix;  
&emsp;Vector:  
&emsp;&emsp;<ins>Creational:</ins>  
&emsp;&emsp;**vector**(n) - creates an empty vector with length n;  
&emsp;&emsp;**vector_hp**(n) - creates an empty high performance (hp) vector with length n;  
&emsp;&emsp;**range**(x1; xn; s) - creates a vector with values spanning from x1 to xn with step s;  
&emsp;&emsp;**range_hp**(x1; xn; s) - creates a high performance (hp) from a range of values as above;  
&emsp;&emsp;<ins>Structural:</ins>  
&emsp;&emsp;**len**(v) - returns the length of vector v;  
&emsp;&emsp;**size**(v) - the actual size of vector v - the index of the last non-zero element;  
&emsp;&emsp;**resize**(v; n) - sets a new length n of vector v;  
&emsp;&emsp;**fill**(v; x) - fills vector v with value x;  
&emsp;&emsp;**join**(A; b; c…) - creates a vector by joining the arguments: matrices, vectors and scalars;  
&emsp;&emsp;**slice**(v; i₁; i₂) - returns the part of vector v bounded by indexes i₁ and i₂ inclusive;  
&emsp;&emsp;**first**(v; n) - the first n elements of vector v;  
&emsp;&emsp;**last**(v; n) - the last n elements of vector v;  
&emsp;&emsp;**extract**(v; i) - extracts the elements from v which indexes are contained in i;  
&emsp;&emsp;<ins>Data:</ins>  
&emsp;&emsp;**sort**(v) - sorts the elements of vector v in ascending order;  
&emsp;&emsp;**rsort**(v) - sorts the elements of vector v in descending order;  
&emsp;&emsp;**order**(v) - the indexes of vector v, arranged by the ascending order of its elements;  
&emsp;&emsp;**revorder**(v) - the indexes of vector v, arranged by the descending order of its elements;  
&emsp;&emsp;**reverse**(v) - a new vector containing the elements of v in reverse order;  
&emsp;&emsp;**count**(v; x; i) - the number of elements in v, after the i-th one, that are equal to x;  
&emsp;&emsp;**search**(v; x; i)- the index of the first element in v, after the i-th one, that is equal to x;  
&emsp;&emsp;**find**(v; x; i) or  
&emsp;&emsp;**find_eq**(v; x; i) - the indexes of all elements in v, after the i-th one, that are = x;  
&emsp;&emsp;**find_ne**(v; x; i) - the indexes of all elements in v, after the i-th one, that are ≠ x;  
&emsp;&emsp;**find_lt**(v; x; i) - the indexes of all elements in v, after the i-th one, that are < x;  
&emsp;&emsp;**find_le**(v; x; i) - the indexes of all elements in v, after the i-th one, that are ≤ x;  
&emsp;&emsp;**find_gt**(v; x; i) - the indexes of all elements in v, after the i-th one, that are > x;  
&emsp;&emsp;**find_ge**(v; x; i) - the indexes of all elements in v, after the i-th one, that are ≥ x;  
&emsp;&emsp;**lookup**(a; b; x) or  
&emsp;&emsp;**lookup_eq**(a; b; x) - all elements in a for which the respective elements in b are = x;  
&emsp;&emsp;**lookup_ne**(a; b; x) - all elements in a for which the respective elements in b are ≠ x;  
&emsp;&emsp;**lookup_lt**(a; b; x) - all elements in a for which the respective elements in b are < x;  
&emsp;&emsp;**lookup_le**(a; b; x) - all elements in a for which the respective elements in b are ≤ x;  
&emsp;&emsp;**lookup_gt**(a; b; x) - all elements in a for which the respective elements in b are > x;  
&emsp;&emsp;**lookup_ge**(a; b; x) - all elements in a for which the respective elements in b are ≥ x;  
&emsp;&emsp;<ins>Math:</ins>  
&emsp;&emsp;**norm_1**(v) - L1 (Manhattan) norm of vector v;  
&emsp;&emsp;**norm**(v) or  
&emsp;&emsp;**norm_2**(v) or  
&emsp;&emsp;**norm_e**(v) - L2 (Euclidean) norm of vector v;  
&emsp;&emsp;**norm_p**(v; p) - Lp norm of vector v;  
&emsp;&emsp;**norm_i**(v) - L∞ (infinity) norm of vector v;  
&emsp;&emsp;**unit**(v) - the normalized vector v (with L2 norm = 1);  
&emsp;&emsp;**dot**(a; b) - scalar product of two vectors a and b;  
&emsp;&emsp;**cross**(a; b) - cross product of two vectors a and b (with length 2 or 3);  
&emsp;Matrix:  
&emsp;&emsp;<ins>Creational:</ins>  
&emsp;&emsp;**matrix**(m; n) - creates an empty matrix with dimensions m⨯n;  
&emsp;&emsp;**identity**(n) - creates an identity matrix with dimensions n⨯n;  
&emsp;&emsp;**diagonal**(n; d) - creates a n⨯n diagonal matrix and fills the diagonal with value d;  
&emsp;&emsp;**column**(m; c) - creates a column matrix with dimensions m⨯1, filled with value c;  
&emsp;&emsp;**utriang**(n) - creates an upper triangular matrix with dimensions n⨯n;  
&emsp;&emsp;**ltriang**(n) - creates a lower triangular matrix with dimensions n⨯n;  
&emsp;&emsp;**symmetric**(n) - creates a symmetric matrix with dimensions n⨯n;  
&emsp;&emsp;**matrix_hp**(m; n) - creates a high-performance matrix with dimensions m⨯n;  
&emsp;&emsp;**identity_hp**(n) - creates a high-performance identity matrix with dimensions n⨯n;  
&emsp;&emsp;**diagonal_hp**(n; d) - creates a high-performance n⨯n diagonal matrix filled with value d;  
&emsp;&emsp;**column_hp**(m; c) - creates a high-performance m⨯1 column matrix filled with value c;  
&emsp;&emsp;**utriang_hp**(n) - creates a high-performance n⨯n upper triangular matrix;  
&emsp;&emsp;**ltriang_hp**(n) - creates a high-performance n⨯n lower triangular matrix;  
&emsp;&emsp;**symmetric_hp**(n) - creates a high-performance symmetric matrix with dimensions n⨯n;  
&emsp;&emsp;**vec2diag**(v) - creates a diagonal matrix from the elements of vector v;  
&emsp;&emsp;**vec2row**(v) - creates a row matrix from the elements of vector v;   
&emsp;&emsp;**vec2col**(v) - creates a column matrix from the elements of vector v;  
&emsp;&emsp;**join_cols**(c₁; c₂; c₃…) - creates a new matrix by joining column vectors;  
&emsp;&emsp;**join_rows**(r₁; r₂; r₃…) - creates a new matrix by joining row vectors;  
&emsp;&emsp;**augment**(A; B; C…) - creates a new matrix by appending matrices A; B; C side by side;  
&emsp;&emsp;**stack**(A; B; C…) - creates a new matrix by stacking matrices A; B; C one below the other;  
&emsp;&emsp;<ins>Structural:</ins>  
&emsp;&emsp;**n_rows**(M) - number of rows in matrix M;  
&emsp;&emsp;**n_cols**(M) - number of columns in matrix M;  
&emsp;&emsp;**mresize**(M; m; n) - sets new dimensions m and n for matrix M;  
&emsp;&emsp;**mfill**(M; x) - fills matrix M with value x;  
&emsp;&emsp;**fill_row**(M; i; x) - fills the i-th row of matrix M with value x;  
&emsp;&emsp;**fill_col**(M; j; x) - fills the j-th column of matrix M with value x;  
&emsp;&emsp;**copy**(A; B; i; j) - copies all elements from A to B, starting from indexes i and j of B;  
&emsp;&emsp;**add**(A; B; i; j) - adds all elements from A to those of B, starting from indexes i and j of B;  
&emsp;&emsp;**row**(M; i) - extracts the i-th row of matrix M as a vector;  
&emsp;&emsp;**col**(M; j) - extracts the j-th column of matrix M as a vector;  
&emsp;&emsp;**extract_rows**(M; i) - extracts the rows from matrix M whose indexes are contained in vector i;  
&emsp;&emsp;**extract_cols**(M; j) - extracts the columns from matrix M whose indexes are contained in vector j;  
&emsp;&emsp;**diag2vec**(M) - extracts the diagonal elements of matrix M to a vector;  
&emsp;&emsp;**submatrix**(M; i₁; i₂; j₁; j₂) - extracts a submatrix of M, bounded by rows i₁ and i₂ and columns j₁ and j₂, incl.;  
&emsp;&emsp;<ins>Data:</ins>  
&emsp;&emsp;**sort_cols**(M; i) - sorts the columns of M based on the values in row i in ascending order;  
&emsp;&emsp;**rsort_cols**(M; i) - sorts the columns of M based on the values in row i in descending order;  
&emsp;&emsp;**sort_rows**(M; j) - sorts the rows of M based on the values in column j in ascending order;  
&emsp;&emsp;**rsort_rows**(M; j) - sorts the rows of M based on the values in column j in descending order;  
&emsp;&emsp;**order_cols**(M; i) - the indexes of the columns of M based on the ordering of the values from row i in ascending order;  
&emsp;&emsp;**revorder_cols**(M; i) - the indexes of the columns of M based on the ordering of the values from row i in descending order;  
&emsp;&emsp;**order_rows**(M; j) - the indexes of the rows of M based on the ordering of the values in column j in ascending order;  
&emsp;&emsp;**revorder_rows**(M; j) - the indexes of the rows of M based on the ordering of the values in column j in descending order;  
&emsp;&emsp;**mcount**(M; x) - number of occurrences of value x in matrix M;  
&emsp;&emsp;**msearch**(M; x; i; j) - vector with the two indexes of the first occurrence of x in matrix M, starting from indexes i and j;  
&emsp;&emsp;**mfind**(M; x) or  
&emsp;&emsp;**mfind_eq**(M; x) - the indexes of all elements in M that are = x;  
&emsp;&emsp;**mfind_ne**(M; x) - the indexes of all elements in M that are ≠ x;  
&emsp;&emsp;**mfind_lt**(M; x) - the indexes of all elements in M that are < x;  
&emsp;&emsp;**mfind_le**(M; x) - the indexes of all elements in M that are ≤ x;  
&emsp;&emsp;**mfind_gt**(M; x) - the indexes of all elements in M that are > x;  
&emsp;&emsp;**mfind_ge**(M; x) - the indexes of all elements in M that are ≥ x;  
&emsp;&emsp;**hlookup**(M; x; i₁; i₂) or  
&emsp;&emsp;**hlookup_eq**(M; x; i₁; i₂) - the values from row i₂ of M, for which the elements in row i₁ are = x;  
&emsp;&emsp;**hlookup_ne**(M; x; i₁; i₂) - the values from row i₂ of M, for which the elements in row i₁ are ≠ x;  
&emsp;&emsp;**hlookup_lt**(M; x; i₁; i₂) - the values from row i₂ of M, for which the elements in row i₁ are < x;  
&emsp;&emsp;**hlookup_le**(M; x; i₁; i₂) - the values from row i₂ of M, for which the elements in row i₁ are ≤ x;  
&emsp;&emsp;**hlookup_gt**(M; x; i₁; i₂) - the values from row i₂ of M, for which the elements in row i₁ are > x;  
&emsp;&emsp;**hlookup_ge**(M; x; i₁; i₂) - the values from row i₂ of M, for which the elements in row i₁ are ≥ x;  
&emsp;&emsp;**vlookup**(M; x; j₁; j₂) or  
&emsp;&emsp;**vlookup_eq**(M; x; j₁; j₂) - the values from column j₂ of M, for which the elements in column j₁ are = x;  
&emsp;&emsp;**vlookup_ne**(M; x; j₁; j₂) - the values from column j₂ of M, for which the elements in column j₁ are ≠ x;  
&emsp;&emsp;**vlookup_lt**(M; x; j₁; j₂) - the values from column j₂ of M, for which the elements in column j₁ are < x;  
&emsp;&emsp;**vlookup_le**(M; x; j₁; j₂) - the values from column j₂ of M, for which the elements in column j₁ are ≤ x;  
&emsp;&emsp;**vlookup_gt**(M; x; j₁; j₂) - the values from column j₂ of M, for which the elements in column j₁ are > x;  
&emsp;&emsp;**vlookup_ge**(M; x; j₁; j₂) - the values from column j₂ of M, for which the elements in column j₁ are ≥ x;  
&emsp;&emsp;<ins>Math:</ins>  
&emsp;&emsp;**hprod**(A; B) - Hadamard product of matrices A and B;  
&emsp;&emsp;**fprod**(A; B) - Frobenius product of matrices A and B;  
&emsp;&emsp;**kprod**(A; B) - Kronecker product of matrices A and B;  
&emsp;&emsp;**mnorm_1**(M) - L1 norm of matrix M;  
&emsp;&emsp;**mnorm**(M) or  
&emsp;&emsp;**mnorm_2**(M) - L2 norm of matrix M;  
&emsp;&emsp;**mnorm_e**(M) - Frobenius norm of matrix M;  
&emsp;&emsp;**mnorm_i**(M) - L∞ norm of matrix M;  
&emsp;&emsp;**cond_1**(M) - condition number of M based on the L1 norm;  
&emsp;&emsp;**cond**(M) or  
&emsp;&emsp;**cond_2**(M) - condition number of M based on the L2 norm;  
&emsp;&emsp;**cond_e**(M) - condition number of M based on the Frobenius norm;  
&emsp;&emsp;**cond_i**(M) - condition number of M based on the L∞ norm;  
&emsp;&emsp;**det**(M) - determinant of matrix M;  
&emsp;&emsp;**rank**(M) - rank of matrix M;  
&emsp;&emsp;**trace**(M) - trace of matrix M;  
&emsp;&emsp;**transp**(M) - transpose of matrix M;  
&emsp;&emsp;**adj**(M) - adjugate of matrix M;  
&emsp;&emsp;**cofactor**(M) - cofactor matrix of M;  
&emsp;&emsp;**eigenvals**(M; n_e) - the first n_e eigenvalues of matrix M (or all if omitted);  
&emsp;&emsp;**eigenvecs**(M; n_e) - the first n_e eigenvectors of matrix M (or all if omitted);  
&emsp;&emsp;**eigen**(M; n_e) - the first n_e eigenvalues and eigenvectors of M (or all if omitted);  
&emsp;&emsp;**cholesky**(M) - Cholesky decomposition of a symmetric, positive-definite matrix M;  
&emsp;&emsp;**lu**(M) - LU decomposition of matrix M;  
&emsp;&emsp;**qr**(M) - QR decomposition of matrix M;  
&emsp;&emsp;**svd**(M) - singular value decomposition of M;  
&emsp;&emsp;**inverse**(M) - inverse of matrix M;  
&emsp;&emsp;**lsolve**(A; b) - solves the system of linear equations Ax = b using LDLT decomposition for symmetric matrices, and LU for non-symmetric;  
&emsp;&emsp;**clsolve**(A; b) - solves the linear matrix equation Ax = b with symmetric, positive-definite matrix A using Cholesky decomposition;  
&emsp;&emsp;**slsolve**(A; b) - solves the linear matrix equation Ax = b with high-performance symmetric, positive-definite matrix A using preconditioned conjugate gradient (PCG) method;  
&emsp;&emsp;**msolve**(A; B) - solves the generalized matrix equation AX = B using LDLT decomposition for symmetric matrices, and LU for non-symmetric;  
&emsp;&emsp;**cmsolve**(A; B) - solves the generalized matrix equation AX = B with symmetric, positive-definite matrix A using Cholesky decomposition;  
&emsp;&emsp;**smsolve**(A; B) - solves the generalized matrix equation AX = B with high-performance symmetric, positive-definite matrix A using PCG method;  
&emsp;&emsp;**matmul**(A; B) - fast multiplication of square hp matrices using parallel Winograd algorithm. The multiplication operator A*B uses it automatically for square matrices of size 1000 and larger;
&emsp;&emsp;**fft**(M) - performs fast Fourier transform of row-major matrix M. It must have one row for real data and two rows for complex;  
&emsp;&emsp;**ift**(M) - performs inverse Fourier transform of row-major matrix M. It must have one row for real data and two rows for complex;  
&emsp;&emsp;**<ins>Double interpolation:</ins>**  
&emsp;&emsp;**take**(x; y; M) - returns the element of matrix M at indexes x and y;  
&emsp;&emsp;**line**(x; y; M) - double linear interpolation from the elements of matrix M based on the values of x and y;  
&emsp;&emsp;**spline**(x; y; M) - double Hermite spline interpolation from the elements of matrix M based on the values of x and y.  
&emsp;&emsp;*Tol* - target tolerance for the iterative PCG solver.  
* Comments: "Title" or 'text' in double or single quotes, respectively. HTML, CSS, JS and SVG are allowed.  
* Graphing and plotting:  
&emsp;$Plot { f(x) @ x = a : b } - simple plot;  
&emsp;$Plot { x(t) | y(t) @ t = a : b } - parametric;  
&emsp;$Plot { f1(x) & f2(x) & ... @ x = a : b } - multiple;  
&emsp;$Plot { x1(t) | y1(t) & x2(t) | y2(t) & ... @ x = a : b } - multiple parametric;  
&emsp;$Map { f(x; y) @ x = a : b & y = c : d }  - 2D color map of a 3D surface;  
&emsp;PlotHeight - height of plot area in pixels;  
&emsp;PlotWidth - width of plot area in pixels;  
&emsp;PlotSVG - draw plots in vector (SVG) format;  
&emsp;PlotAdaptive - use adaptive mesh (= 1) for function plotting or uniform (= 0);  
&emsp;PlotStep - the size of the mesh for map plotting;  
&emsp;PlotPalette - the number of color palette to be used for surface plots (0-9);  
&emsp;PlotShadows - draw surface plots with shadows;  
&emsp;PlotSmooth - smooth transition of colors (= 1) or isobands (= 0) for surface plots;  
&emsp;PlotLightDir - direction to light source (0-7) clockwise.  
* Iterative and numerical methods:  
&emsp;$Root { f(x) = const @ x = a : b } - root finding for f(x) = const;  
&emsp;$Root { f(x) @ x = a : b } - root finding for f(x) = 0;  
&emsp;$Find { f(x) @ x = a : b } - similar to above, but x is not required to be a precise solution;  
&emsp;$Sup { f(x) @ x = a : b } - local maximum of a function;  
&emsp;$Inf { f(x) @ x = a : b } - local minimum of a function;  
&emsp;$Area { f(x) @ x = a : b } - adaptive Gauss-Lobatto numerical integration;  
&emsp;$Integral { f(x) @ x = a : b } - Tanh-Sinh numerical integration;  
&emsp;$Slope { f(x) @ x = a } - numerical differentiation by Richardson extrapolation;  
&emsp;$Derivative { f(x) @ x = a } - numerical differentiation by complex step method;  
&emsp;$Sum { f(k) @ k = a : b } - iterative sum;  
&emsp;$Product { f(k) @ k = a : b } - iterative product;  
&emsp;$Repeat { f(k) @ k = a : b } - iterative expression block with counter;  
&emsp;$While{condition; expressions} - iterative expression block with condition;  
&emsp;$Block{expressions} - multiline expression block;  
&emsp;$Inline{expressions} - inline expression block;  
&emsp;Precision - relative precision for numerical methods \[10<sup>-2</sup>; 10<sup>-16</sup>\] (default is 10<sup>-12</sup>)   
* Program flow control:  
&emsp;Simple:  
&emsp;&emsp;#if *condition*  
&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;#end if  
&emsp;Alternative:  
&emsp;&emsp;#if *condition*  
&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;#else  
&emsp;&emsp;&emsp;*Some other code*  
&emsp;&emsp;#end if  
&emsp;Complete:  
&emsp;&emsp;#if *condition1*  
&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;#else if *condition2*  
&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;#else  
&emsp;&emsp;&emsp;*Some other code*  
&emsp;&emsp;#end if  
You can add or omit as many "#else if's" as needed. Only one "#else" is allowed. You can omit this too.  
* Iteration blocks:  
&emsp;Simple:  
&emsp;&emsp;#repeat *number of repetitions*  
&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;#loop  
&emsp;With conditional break/coutinue:  
&emsp;&emsp;#repeat *number of repetitions*  
&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;&emsp;#if *condition*  
&emsp;&emsp;&emsp;&emsp;#break or #continue  
&emsp;&emsp;&emsp;#end if  
&emsp;&emsp;&emsp;*Some more code*  
&emsp;&emsp;#loop  
&emsp;With counter:  
&emsp;&emsp;#for counter = start : end  
&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;#loop  
&emsp;With condition:  
&emsp;&emsp;#while *condition*  
&emsp;&emsp;&emsp;*Your code goes here*  
&emsp;&emsp;#loop  
* Modules and macros/string variables:  
&emsp;Modules:  
&emsp;&emsp;#include *filename* - include external file (module);  
&emsp;&emsp;#local - start local section (not to be included);  
&emsp;&emsp;#global - start global section (to be included);  
&emsp;Inline string variable:  
&emsp;&emsp;#def *variable_name$* = *content*  
&emsp;Multiline string variable:  
&emsp;&emsp;#def *variable_name$*  
&emsp;&emsp;&emsp;*content line 1*  
&emsp;&emsp;&emsp;*content line 2*  
&emsp;&emsp;&emsp;...  
&emsp;&emsp;#end def  
&emsp;Inline string macro:  
&emsp;&emsp;#def *macro_name$*(*param1$*; *param2$*;...) = *content*  
&emsp;Multiline string macro:  
&emsp;&emsp;#def *macro_name$*(*param1$*; *param2$*;...)  
&emsp;&emsp;&emsp;*content line 1*  
&emsp;&emsp;&emsp;*content line 2*  
&emsp;&emsp;&emsp;...  
&emsp;&emsp;#end def  
* Import/Export of external data:  
&emsp;Text/CSV files:  
&emsp;&emsp;#read M from filename.txt@R1C1:R2C2 TYPE=R SEP=',' - read matrix M from a text/CSV file;  
&emsp;&emsp;#write M to filename.txt@R1C1:R2C2 TYPE=N SEP=',' - write matrix M to a text/CSV file;  
&emsp;&emsp;#append M to filename.txt@R1C1:R2C2 TYPE=N SEP=',' - append matrix M to a text/CSV file;  
&emsp;Excel files (xlsx and xlsm):  
&emsp;&emsp;#read M from filename.xlsx@Sheet1!A1:B2 TYPE=R - read matrix M from an Excel file;  
&emsp;&emsp;#write M to filename.xlsx@Sheet1!A1:B2 TYPE=N - write matrix M to an Excel file;  
&emsp;&emsp;#append M to filename.xlsx@Sheet1!A1:B2 TYPE=N - append matrix M to an Excel file (same as write);  
&emsp;Sheet, range, TYPE and SEP can be omitted.  
&emsp;For #read command, TYPE can be either of [R|D|C|S|U|L|V].  
&emsp;For #write and #append commands, TYPE can be Y or N.  
* Output control:  
&emsp;#hide - hide the report contents;  
&emsp;#show - always show the contents (default);  
&emsp;#pre  - show the next contents only before calculations;  
&emsp;#post - show the next contents only after calculations;  
&emsp;#val  - show only the final result, without the equation;  
&emsp;#equ  - show complete equations and results (default);  
&emsp;#noc  - show only equations without results (no calculations);  
&emsp;#nosub  - do not substitute variables (no substitution);  
&emsp;#novar  - show equations only with substituted values (no variables);  
&emsp;#varsub - show equations with variables and substituted values (default);  
&emsp;#split - split equations that do not fit on a single line;  
&emsp;#wrap - wrap equations that do not fit on a single line (default);  
&emsp;#round n - rounds the output to n digits after the decimal point;  
&emsp;#round default - restores rounding to the default settings;  
&emsp;#format FFFF - specifies custom format string;  
&emsp;#format default - restores the default formatting;  
&emsp;#md on - enables markdown in comments;  
&emsp;#md off - disables markdown in comments;  
&emsp;#phasor - sets output format of complex numbers to polar phasor: A∠φ;  
&emsp;#complex - sets output format of complex numbers to Cartesian algebraic: a + bi.  
&emsp;Each of the above commands is effective after the current line until the end of the report or another command that overwrites it.  
* Breakpoints for step-by-step execution:  
&emsp;#pause - calculates to the current line and waits until resumed manually;  
&emsp;#input - renders an input form to the current line and waits for user input.  
* Switches for trigonometric units: #deg - degrees, #rad - radians, #gra - gradians;  
* Separator for target units: |, for example:  `3ft + 12in|cm` will show 121.92 cm;  
* Dimensionless: %, ‰, ‱, pcm, ppm, ppb, ppt, ppq;  
* Angle units: °, ′, ″, deg, rad, grad, rev;  
* Metric units (SI and compatible):  
&emsp;Mass: g, hg, kg, t, kt, Mt, Gt, dg, cg, mg, μg, Da, u;  
&emsp;Length: m, km, dm, cm, mm, μm, nm, pm, AU, ly;  
&emsp;Time: s, ms, μs, ns, ps, min, h, d, w, y;  
&emsp;Frequency: Hz, kHz, MHz, GHz, THz, mHz, μHz, nHz, pHz, rpm;  
&emsp;Speed: kmh;  
&emsp;Electric current: A, kA, MA, GA, TA, mA, μA, nA, pA;  
&emsp;Temperature: °C, Δ°C, K;  
&emsp;Amount of substance: mol;  
&emsp;Luminous intensity: cd;  
&emsp;Area: a, daa, ha;  
&emsp;Volume: L, daL, hL, dL, cL, mL, μL, nL, pL;  
&emsp;Force: dyn N, daN, hN, kN, MN, GN, TN, gf, kgf, tf;  
&emsp;Moment: Nm, kNm;  
&emsp;Pressure: Pa, daPa, hPa, kPa, MPa, GPa, TPa, dPa, cPa, mPa, μPa, nPa, pPa,  
&emsp;&emsp;&emsp; bar, mbar, μbar, atm, at, Torr, mmHg;  
&emsp;Viscosity: P, cP, St, cSt;  
&emsp;Energy work: J, kJ, MJ, GJ, TJ, mJ, μJ, nJ, pJ,  
&emsp;&emsp;&emsp;&emsp;Wh, kWh, MWh, GWh, TWh, mWh, μWh, nWh, pWh  
&emsp;&emsp;&emsp;&emsp;eV, keV, MeV, GeV, TeV, PeV, EeV, cal, kcal, erg;  
&emsp;Power: W, kW, MW, GW, TW, mW, μW, nW, pW, hpM, ks;  
&emsp;&emsp;&emsp; VA, kVA, MVA, GVA, TVA, mVA, μVA, nVA, pVA,  
&emsp;&emsp;&emsp; VAR, kVAR, MVAR, GVAR, TVAR, mVAR, μVAR, nVAR, pVAR, hpM, ks;  
&emsp;Electric charge: C, kC, MC, GC, TC, mC, μC, nC, pC, Ah, mAh;  
&emsp;Potential: V, kV, MV, GV, TV, mV, μV, nV, pV;  
&emsp;Capacitance: F, kF, MF, GF, TF, mF, μF, nF, pF;  
&emsp;Resistance: Ω, kΩ, MΩ, GΩ, TΩ, mΩ, μΩ, nΩ, pΩ;  
&emsp;Conductance: S, kS, MS, GS, TS, mS, μS, nS, pS, ℧, k℧, M℧, G℧, T℧, m℧, μ℧, n℧, p℧;  
&emsp;Magnetic flux: Wb , kWb, MWb, GWb, TWb, mWb, μWb, nWb, pWb;  
&emsp;Magnetic flux density: T, kT, MT, GT, TT, mT, μT, nT, pT;  
&emsp;Inductance: H, kH, MH, GH, TH, mH, μH, nH, pH;  
&emsp;Luminous flux: lm;  
&emsp;Illuminance: lx;  
&emsp;Radioactivity: Bq, kBq, MBq, GBq, TBq, mBq, μBq, nBq, pBq, Ci, Rd;  
&emsp;Absorbed dose: Gy, kGy, MGy, GGy, TGy, mGy, μGy, nGy, pGy;  
&emsp;Equivalent dose: Sv, kSv, MSv, GSv, TSv, mSv, μSv, nSv, pSv;  
&emsp;Catalytic activity: kat;  
* Non-metric units (Imperial/US):  
&emsp;Mass: gr, dr, oz, lb (or lbm, lb_m), klb, kipm (or kip_m), st, qr,  
&emsp;&emsp;&ensp; cwt (or cwt_UK, cwt_US), ton (or ton_UK, ton_US), slug;  
&emsp;Length: th, in, ft, yd, ch, fur, mi, ftm (or ftm_UK, ftm_US),  
&emsp;&emsp;&emsp;&ensp; cable (or cable_UK, cable_US), nmi, li, rod, pole, perch, lea;  
&emsp;Speed: mph, knot;  
&emsp;Temperature: °F, Δ°F, °R;  
&emsp;Area: rood, ac;  
&emsp;Volume, fluid: fl_oz, gi, pt, qt, gal, bbl, or:  
&emsp;&emsp;&emsp;fl_oz_UK, gi_UK, pt_UK, qt_UK, gal_UK, bbl_UK,  
&emsp;&emsp;&emsp;fl_oz_US, gi_US, pt_US, qt_US, gal_US, bbl_US,  
&emsp;Volume, dry: (US) pt_dry, (US) qt_dry, (US) gal_dry, (US) bbl_dry,  
&emsp;&emsp;&emsp;pk (or pk_UK, pk_US), bu (or bu_UK, bu_US);  
&emsp;Force: ozf (or oz_f), lbf (or lb_f), kip (or kipf, kip_f), tonf (or ton_f), pdl;  
&emsp;Pressure: osi, osf psi, psf, ksi, ksf, tsi, tsf, inHg;  
&emsp;Energy/work: BTU, therm, (or therm_UK, therm_US), quad;  
&emsp;Power: hp, hpE, hpS;  
* Custom units - .Name = expression.  
Names can include currency symbols: €, £, ₤, ¥, ¢, ₽, ₹, ₩, ₪.
