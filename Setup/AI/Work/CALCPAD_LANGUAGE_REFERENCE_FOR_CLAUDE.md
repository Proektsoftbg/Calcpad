# Calcpad Language Reference for Claude Code

---

## Description

Calcpad is free software for mathematical and engineering calculations. It represents a flexible and modern programmable calculator with Html report generator. It is simple and easy to use, but it also includes many advanced features:  
  
- real and complex numbers (rectangular and polar-phasor formats);
- units of measurement (SI, Imperial and USCS);
- vectors and matrices: rectangular, symmetric, column, diagonal, upper/lower triangular;
- custom variables and units;
- built-in library with common math functions;
- vectors and matrix functions:
  - data functions: search, lookup, sort, count, etc.;
  - aggregate functions: min, max, sum, sumsq, srss, average, product (geometric) mean, etc.;
  - math functions: norm, condition, determinant, rank, trace, transpose, adjugate and cofactor, inverse, factorization (cholesky, ldlt, lu, qr and svd), eigenvalues/vectors and linear systems of equations;
- custom functions of multiple parameters f(x; y; z; …);
- powerful numerical methods for root and extremum finding, numerical integration and differentiation;
- finite sum, product and iteration procedures, Fourier series and FFT;
- modules, macros and string variables;
- reading and writing data from/to text, CSV and Excel files;
- program flow control with conditions and loops;
- "titles" and 'text' comments in quotes;
- support for Html and CSS in comments for rich formatting;
- function plotting, images, tables, parametric SVG drawings, etc.;
- automatic generation of Html forms for data input;
- professional looking Html reports for viewing and printing;
- export to Word (\*.docx) and PDF documents;
- variable substitution and smart rounding of numbers;
- output visibility control and content folding;
- support for plain text (\*.txt, \*.cpd) and binary (\*.cpdz) file formats.

---

## Language specification  

### Primitives:  

  - Real numbers: digits 0 - 9 and decimal point ".", e.g. `3.14`  
  - Complex numbers: re ± imi, e.g. `3 - 2i`  
  - Vectors: `[v₁; v₂; v₃; …; vₙ]`  
  - Matrices: `[M₁₁; M₁₂; … ; M₁ₙ | M₂₁; M₂₂; … ; M₂ₙ | … | Mₘ₁; Mₘ₂; … ; Mₘₙ]`  

### Variables:  

  - all Unicode letters  
  - digits: 0 - 9  
  - comma: " , "  
  - special symbols: ′ , ″ , ‴ , ⁗ , ‾ , ø , Ø , ° , ∡   
  - superscripts: ⁰ , ¹ , ² , ³ , ⁴ , ⁵ , ⁶ , ⁷ , ⁸ , ⁹ , ⁿ , ⁺ , ⁻   
  - subscripts: ₀ , ₁ , ₂ , ₃ , ₄ , ₅ , ₆ , ₇ , ₈ , ₉ , ₊ , ₋ , ₌ , ₍ , ₎  
  - " _ " for subscript  
  Any variable name must start with a letter. Names are case sensitive.  

### Operators:  

  `!` - factorial  
  `^` - exponent  
  `/` - division  
  `÷` - force division bar in inline mode and slash in pro mode (//)  
  `\`\ - integer division  
  `⦼` - modulo (remainder)  
  `\`* - multiplication  
  `-` - minus  
  `+` - plus  
  `≡` - equal to  
  `≠` - not equal to  
  `<` - less than  
  `>` - greater than  
  `≤` - less or equal  
  `≥` - greater or equal  
  `∧` - logical "AND"   
  `∨` - logical "OR"   
  `⊕` - logical "XOR"   
  `∠` - phasor A∠φ (<<)   
  `=` - assignment  
  
### Custom functions: 

  Definition: `f(x; y; z; ... ) = expression`  
  Delimiter for arguments: semicolon `;`  
  
### Built-in functions:  

#### Trigonometric:  

  `sin(x)`  - sine  
  `cos(x)`  - cosine  
  `tan(x)`  - tangent  
  `csc(x)`  - cosecant  
  `sec(x)`  - secant  
  `cot(x)`  - cotangent  
  
#### Hyperbolic:  

  `sinh(x)` - hyperbolic sine  
  `cosh(x)` - hyperbolic cosine  
  `tanh(x)` - hyperbolic tangent  
  `csch(x)` - hyperbolic cosecant  
  `sech(x)` - hyperbolic secant  
  `coth(x)` - hyperbolic cotangent  
  
#### Inverse trigonometric:  

  `asin(x)` - inverse sine  
  `acos(x)` - inverse cosine  
  `atan(x)` - inverse tangent  
  `atan2(x; y)` - the angle whose tangent is the quotient of y and x  
  `acsc(x)` - inverse cosecant  
  `asec(x)` - inverse secant  
  `acot(x)` - inverse cotangent  
  
#### Inverse hyperbolic:  

  `asinh(x)` - inverse hyperbolic sine  
  `acosh(x)` - inverse hyperbolic cosine  
  `atanh(x)` - inverse hyperbolic tangent  
  `acsch(x)` - inverse hyperbolic cosecant  
  `asech(x)` - inverse hyperbolic secant  
  `acoth(x)` - inverse hyperbolic cotangent  
  
#### Logarithmic, exponential and roots:  

  `log(x)`   - decimal logarithm  
  `ln(x)`    - natural logarithm  
  `log_2(x)` - binary logarithm  
  `exp(x)`   - natural exponent  
  `sqr(x)` or `sqrt(x)` - square root  
  `cbrt(x)` - cubic root  
  `root(x; n)` - n-th root  
  
#### Rounding:  

  `round(x)` - round to the nearest integer  
  `floor(x)` - round to the smaller integer (towards -∞)  
  `ceiling(x)` - round to the greater integer (towards +∞)  
  `trunc(x)` - round to the smaller integer (towards zero)  
  
#### Integer:  

  `mod(x; y)` - the remainder of an integer division  
  `gcd(x; y; z...)` - the greatest common divisor of several integers  
  `lcm(x; y; z...)` - the least common multiple of several integers  
  
#### Complex:  

  `re(z)` - the real part of a complex number  
  `im(z)` - the imaginary part of a complex number  
  `abs(z)` - absolute value/magnitude  
  `phase(z)` - the phase of a complex number  
  `conj(z)`- the conjugate of a complex number  
  
#### Aggregate and interpolation:  

  `min(x; y; z...)` - minimum of multiple values  
  `max(x; y; z...)` - maximum of multiple values  
  `sum(x; y; z...)` - sum of multiple values = x + y + z...  
  `sumsq(x; y; z...)` - sum of squares = x² + y² + z²...  
  `srss(x; y; z...)` - square root of sum of squares = sqrt(x² + y² + z²...)  
  `average(x; y; z...)` - average of multiple values = (x + y + z...)/n  
  `product(x; y; z...)` - product of multiple values = x·y·z...  
  `mean(x; y; z...)` - geometric mean = n-th root(x·y·z...)  
  `take(n; a; b; c...)` - returns the n-th element from the list  
  `line(x; a; b; c...)` - linear interpolation  
  `spline(x; a; b; c...)` - Hermite spline interpolation  
  
#### Conditional and logical:  

  `if(condition; value-if-true; value-if-false)` - conditional evaluation  
  `switch(condition1; value1; condition2; value2; … ; default)` - selective evaluation  
  `not(x)` - logical "NOT"  
  `and(x; y; z...)` - logical "AND"  
  `or(x; y; z...)` - logical "OR"  
  `xor(x; y; z...)` - logical "XOR"  
  
#### Other:  

  `sign(x)` - the sign of a number  
  `random(x)` - random number between 0 and x  
  `getunits(x)` - gets the units of x without the value. Returns 1 if x is unitless  
  `setunits(x; u)` - sets the units u to x where x can be scalar, vector or matrix  
  `clrunits(x)` - clears the units from a scalar, vector or matrix x  
  `hp(x)` - converts x to its high performance (hp) equivalent type  
  `ishp(x)` - checks if the type of x is a high-performance (hp) vector or matrix  
  
#### Vector:  

##### Creational: 

  `vector(n)` - creates an empty vector with length n  
  `vector_hp(n)` - creates an empty high performance (hp) vector with length n  
  `range(x1; xn; s)` - creates a vector with values spanning from x1 to xn with step s  
  `range_hp(x1; xn; s)` - creates a high performance (hp) from a range of values as above  
		
##### Structural: 

  `len(v)` - returns the length of vector v  
  `size(v)` - the actual size of vector v - the index of the last non-zero element  
  `resize(v; n)` - sets a new length n of vector v  
  `fill(v; x)` - fills vector v with value x  
  `join(A; b; c…)` - creates a vector by joining the arguments: matrices, vectors and scalars  
  `slice(v; i₁; i₂)` - returns the part of vector v bounded by indexes i₁ and i₂ inclusive  
  `first(v; n)` - the first n elements of vector v  
  `last(v; n)` - the last n elements of vector v  
  `extract(v; i)` - extracts the elements from v which indexes are contained in i  
		
##### Data: 

  `sort(v)` - sorts the elements of vector v in ascending order  
  `rsort(v)` - sorts the elements of vector v in descending order  
  `order(v)` - the indexes of vector v, arranged by the ascending order of its elements  
  `revorder(v)` - the indexes of vector v, arranged by the descending order of its elements  
  `reverse(v)` - a new vector containing the elements of v in reverse order  
  `count(v; x; i)` - the number of elements in v, after the i-th one, that are equal to x  
  `search(v; x; i)` - the index of the first element in v, after the i-th one, that is equal to x  
  `find(v; x; i) or  
  `find_eq(v; x; i)` - the indexes of all elements in v, after the i-th one, that are = x  
  `find_ne(v; x; i)` - the indexes of all elements in v, after the i-th one, that are ≠ x  
  `find_lt(v; x; i)` - the indexes of all elements in v, after the i-th one, that are < x  
  `find_le(v; x; i)` - the indexes of all elements in v, after the i-th one, that are ≤ x  
  `find_gt(v; x; i)` - the indexes of all elements in v, after the i-th one, that are > x  
  `find_ge(v; x; i)` - the indexes of all elements in v, after the i-th one, that are ≥ x  
  `lookup(a; b; x) or  
  `lookup_eq(a; b; x)` - all elements in a for which the respective elements in b are = x  
  `lookup_ne(a; b; x)` - all elements in a for which the respective elements in b are ≠ x  
  `lookup_lt(a; b; x)` - all elements in a for which the respective elements in b are < x  
  `lookup_le(a; b; x)` - all elements in a for which the respective elements in b are ≤ x  
  `lookup_gt(a; b; x)` - all elements in a for which the respective elements in b are > x  
  `lookup_ge(a; b; x)` - all elements in a for which the respective elements in b are ≥ x  
  
##### Math: 

  `norm_1(v)` - L1 (Manhattan) norm of vector v  
  `norm(v) or  
  `norm_2(v) or  
  `norm_e(v)` - L2 (Euclidean) norm of vector v  
  `norm_p(v; p)` - Lp norm of vector v  
  `norm_i(v)` - L∞ (infinity) norm of vector v  
  `unit(v)` - the normalized vector v (with L2 norm = 1)  
  `dot(a; b)` - scalar product of two vectors a and b  
  `cross(a; b)` - cross product of two vectors a and b (with length 2 or 3)  
  
#### Matrix:  

##### Creational: 

  `matrix(m; n)` - creates an empty matrix with dimensions m⨯n  
  `identity(n)` - creates an identity matrix with dimensions n⨯n  
  `diagonal(n; d)` - creates a n⨯n diagonal matrix and fills the diagonal with value d  
  `column(m; c)` - creates a column matrix with dimensions m⨯1, filled with value c  
  `utriang(n)` - creates an upper triangular matrix with dimensions n⨯n  
  `ltriang(n)` - creates a lower triangular matrix with dimensions n⨯n  
  `symmetric(n)` - creates a symmetric matrix with dimensions n⨯n  
  `matrix_hp(m; n)` - creates a high-performance matrix with dimensions m⨯n  
  `identity_hp(n)` - creates a high-performance identity matrix with dimensions n⨯n  
  `diagonal_hp(n; d)` - creates a high-performance n⨯n diagonal matrix filled with value d  
  `column_hp(m; c)` - creates a high-performance m⨯1 column matrix filled with value c  
  `utriang_hp(n)` - creates a high-performance n⨯n upper triangular matrix  
  `ltriang_hp(n)` - creates a high-performance n⨯n lower triangular matrix  
  `symmetric_hp(n)` - creates a high-performance symmetric matrix with dimensions n⨯n  
  `vec2diag(v)` - creates a diagonal matrix from the elements of vector v  
  `vec2row(v)` - creates a row matrix from the elements of vector v   
  `vec2col(v)` - creates a column matrix from the elements of vector v  
  `join_cols(c₁; c₂; c₃…)` - creates a new matrix by joining column vectors  
  `join_rows(r₁; r₂; r₃…)` - creates a new matrix by joining row vectors  
  `augment(A; B; C…)` - creates a new matrix by appending matrices A; B; C side by side  
  `stack(A; B; C…)` - creates a new matrix by stacking matrices A; B; C one below the other  
  
##### Structural:  

  `n_rows(M)` - number of rows in matrix M  
  `n_cols(M)` - number of columns in matrix M  
  `mresize(M; m; n)` - sets new dimensions m and n for matrix M  
  `mfill(M; x)` - fills matrix M with value x  
  `fill_row(M; i; x)` - fills the i-th row of matrix M with value x  
  `fill_col(M; j; x)` - fills the j-th column of matrix M with value x  
  `copy(A; B; i; j)` - copies all elements from A to B, starting from indexes i and j of B  
  `add(A; B; i; j)` - adds all elements from A to those of B, starting from indexes i and j of B  
  `row(M; i)` - extracts the i-th row of matrix M as a vector  
  `col(M; j)` - extracts the j-th column of matrix M as a vector  
  `extract_rows(M; i)` - extracts the rows from matrix M whose indexes are contained in vector i  
  `extract_cols(M; j)` - extracts the columns from matrix M whose indexes are contained in vector j  
  `diag2vec(M)` - extracts the diagonal elements of matrix M to a vector  
  `submatrix(M; i₁; i₂; j₁; j₂)` - extracts a submatrix of M, bounded by rows i₁ and i₂ and columns j₁ and j₂, incl.  
		
##### Data: 

  `sort_cols(M; i)` - sorts the columns of M based on the values in row i in ascending order  
  `rsort_cols(M; i)` - sorts the columns of M based on the values in row i in descending order  
  `sort_rows(M; j)` - sorts the rows of M based on the values in column j in ascending order  
  `rsort_rows(M; j)` - sorts the rows of M based on the values in column j in descending order  
  `order_cols(M; i)` - the indexes of the columns of M based on the ordering of the values from row i in ascending order  
  `revorder_cols(M; i)` - the indexes of the columns of M based on the ordering of the values from row i in descending order  
  `order_rows(M; j)` - the indexes of the rows of M based on the ordering of the values in column j in ascending order  
  `revorder_rows(M; j)` - the indexes of the rows of M based on the ordering of the values in column j in descending order  
  `mcount(M; x)` - number of occurrences of value x in matrix M  
  `msearch(M; x; i; j)` - vector with the two indexes of the first occurrence of x in matrix M, starting from indexes i and j  
  `mfind(M; x) or  
  `mfind_eq(M; x)` - the indexes of all elements in M that are = x  
  `mfind_ne(M; x)` - the indexes of all elements in M that are ≠ x  
  `mfind_lt(M; x)` - the indexes of all elements in M that are < x  
  `mfind_le(M; x)` - the indexes of all elements in M that are ≤ x  
  `mfind_gt(M; x)` - the indexes of all elements in M that are > x  
  `mfind_ge(M; x)` - the indexes of all elements in M that are ≥ x  
  `hlookup(M; x; i₁; i₂) or  
  `hlookup_eq(M; x; i₁; i₂)` - the values from row i₂ of M, for which the elements in row i₁ are = x  
  `hlookup_ne(M; x; i₁; i₂)` - the values from row i₂ of M, for which the elements in row i₁ are ≠ x  
  `hlookup_lt(M; x; i₁; i₂)` - the values from row i₂ of M, for which the elements in row i₁ are < x  
  `hlookup_le(M; x; i₁; i₂)` - the values from row i₂ of M, for which the elements in row i₁ are ≤ x  
  `hlookup_gt(M; x; i₁; i₂)` - the values from row i₂ of M, for which the elements in row i₁ are > x  
  `hlookup_ge(M; x; i₁; i₂)` - the values from row i₂ of M, for which the elements in row i₁ are ≥ x  
  `vlookup(M; x; j₁; j₂) or  
  `vlookup_eq(M; x; j₁; j₂)` - the values from column j₂ of M, for which the elements in column j₁ are = x  
  `vlookup_ne(M; x; j₁; j₂)` - the values from column j₂ of M, for which the elements in column j₁ are ≠ x  
  `vlookup_lt(M; x; j₁; j₂)` - the values from column j₂ of M, for which the elements in column j₁ are < x  
  `vlookup_le(M; x; j₁; j₂)` - the values from column j₂ of M, for which the elements in column j₁ are ≤ x  
  `vlookup_gt(M; x; j₁; j₂)` - the values from column j₂ of M, for which the elements in column j₁ are > x  
  `vlookup_ge(M; x; j₁; j₂)` - the values from column j₂ of M, for which the elements in column j₁ are ≥ x  
		
##### Math: 

  `hprod(A; B)` - Hadamard product of matrices A and B  
  `fprod(A; B)` - Frobenius product of matrices A and B  
  `kprod(A; B)` - Kronecker product of matrices A and B  
  `mnorm_1(M)` - L1 norm of matrix M  
  `mnorm(M) or  
  `mnorm_2(M)` - L2 norm of matrix M  
  `mnorm_e(M)` - Frobenius norm of matrix M  
  `mnorm_i(M)` - L∞ norm of matrix M  
  `cond_1(M)` - condition number of M based on the L1 norm  
  `cond(M) or  
  `cond_2(M)` - condition number of M based on the L2 norm  
  `cond_e(M)` - condition number of M based on the Frobenius norm  
  `cond_i(M)` - condition number of M based on the L∞ norm  
  `det(M)` - determinant of matrix M  
  `rank(M)` - rank of matrix M  
  `trace(M)` - trace of matrix M  
  `transp(M)` - transpose of matrix M  
  `adj(M)` - adjugate of matrix M  
  `cofactor(M)` - cofactor matrix of M  
  `eigenvals(M; n_e)` - the first n_e eigenvalues of symmetric matrix M (or all if omitted) as a vector  
  `eigenvecs(M; n_e)` - the first n_e eigenvectors of symmetric matrix M (or all if omitted) along the rows of the returned matrix
  `eigen(M; n_e)` - the first n_e eigenvalues and eigenvectors of symmetric matrix M (or all if omitted). Each row of the returned matrix contains the eigenvalue, followed by the respective egienvector elements  
  `cholesky(M)` - Cholesky decomposition of a symmetric, positive-definite matrix M  
  `lu(M)` - LU decomposition of matrix M  
  `qr(M)` - QR decomposition of matrix M  
  `svd(M)` - singular value decomposition of M  
  `inverse(M)` - inverse of matrix M  
  `lsolve(A; b)` - solves the system of linear equations Ax = b using LDLT decomposition for symmetric matrices, and LU for non-symmetric  
  `clsolve(A; b)` - solves the linear matrix equation Ax = b with symmetric, positive-definite matrix A using Cholesky decomposition  
  `slsolve(A; b)` - solves the linear matrix equation Ax = b with high-performance symmetric, positive-definite matrix A using preconditioned conjugate gradient (PCG) method  
  `msolve(A; B)` - solves the generalized matrix equation AX = B using LDLT decomposition for symmetric matrices, and LU for non-symmetric  
  `cmsolve(A; B)` - solves the generalized matrix equation AX = B with symmetric, positive-definite matrix A using Cholesky decomposition  
  `smsolve(A; B)` - solves the generalized matrix equation AX = B with high-performance symmetric, positive-definite matrix A using PCG method  
  `matmul(A; B)` - fast multiplication of square hp matrices using parallel Winograd algorithm. The multiplication operator A\*B uses it automatically for square hp matrices of size 10 and larger;
  `fft(M)` - performs fast Fourier transform of row-major matrix M. It must have one row for real data and two rows for complex  
  `ift(M)` - performs inverse Fourier transform of row-major matrix M. It must have one row for real data and two rows for complex  
		
##### Double interpolation: 

  `take(x; y; M)` - returns the element of matrix M at indexes x and y  
  `line(x; y; M)` - double linear interpolation from the elements of matrix M based on the values of x and y  
  `spline(x; y; M)` - double Hermite spline interpolation from the elements of matrix M based on the values of x and y  
  `Tol` - target tolerance for the iterative PCG solver  
		
### Comments: 

  "Title" or 'text' in double or single quotes, respectively. HTML, CSS, JS and SVG are allowed inside comments.  
	
### Graphing and plotting:  

  `$Plot{f(x) @ x = a : b}` - simple plot  
  `$Plot{x(t) | y(t) @ t = a : b}` - parametric plot  
  `$Plot{f1(x) & f2(x) & ... @ x = a : b}` - multiple plot  
  `$Plot{x1(t) | y1(t) & x2(t) | y2(t) & ... @ x = a : b}` - multiple parametric plot  
  `$Map{f(x; y) @ x = a : b & y = c : d}` - 2D color map of a 3D surface  
  `PlotHeight` - height of plot area in pixels  
  `PlotWidth` - width of plot area in pixels  
  `PlotSVG` - draw plots in vector (SVG) format  
  `PlotAdaptive` - use adaptive mesh (= 1) for function plotting or uniform (= 0)  
  `PlotStep` - the size of the mesh for map plotting  
  `PlotPalette` - the number of color palette to be used for surface plots (0-9)  
  `PlotShadows` - draw surface plots with shadows  
  `PlotSmooth` - smooth transition of colors (= 1) or isobands (= 0) for surface plots  
  `PlotLightDir` - direction to light source (0-7) clockwise.  
  
### Iterative and numerical methods:  

  `$Root{f(x) = const @ x = a : b}` - root finding for f(x) = const  
  `$Root{f(x) @ x = a : b}` - root finding for f(x) = 0  
  `$Find{f(x) @ x = a : b}` - similar to above, but x is not required to be a precise solution  
  `$Sup{f(x) @ x = a : b}` - local maximum of a function. The location is implicitly assigned to a variable `x_sup`  
  `$Inf{f(x) @ x = a : b}` - local minimum of a function. The location is implicitly assigned to a variable `x_inf`  
  `$Area{f(x) @ x = a : b}` - adaptive Gauss-Lobatto numerical integration  
  `$Integral{f(x) @ x = a : b}` - Tanh-Sinh numerical integration  
  `$Slope{f(x) @ x = a}` - numerical differentiation by Richardson extrapolation (approximate)  
  `$Derivative{f(x) @ x = a}` - numerical differentiation by complex step method (precise)  
  `$Sum{f(k) @ k = a : b}` - iterative sum  
  `$Product{f(k) @ k = a : b}` - iterative product  
  `$Repeat{f(k) @ k = a : b}` - iterative expression block with counter  
  `$While{condition; expressions}` - iterative expression block with condition  
  `$Block{expressions}` - multi-line expression block  
  `$Inline{expressions}` - inline expression block  
  `Precision` - relative precision for numerical methods \[10^-2; 10^-16\] (default is 10^-12)   
	
### Program flow control:  

#### Simple:  

```calcpad
#if *condition*  
  '*Your code goes here* 
#end if  
```
  
#### Alternative:  

```calcpad
#if *condition*  
  '*Your code goes here*  
#else  
  '*Some other code*  
#end if  
 ```

#### Complete:  

```calcpad
#if *condition1*  
  '*Your code goes here*  
#else if *condition2*  
  '*Your code goes here*  
#else  
  '*Some other code*  
#end if  
```
You can add or omit as many "#else if's" as needed. Only one "#else" is allowed. You can omit this too.  

### Iteration blocks:  

#### Simple:  

```calcpad
#repeat *number of repetitions*  
  '*Your code goes here*  
#loop  
```

#### With conditional break/coutinue:  

```calcpad
#repeat *number of repetitions*  
  '*Your code goes here*  
  #if *condition*  
  	#break or #continue  
  #end if  
  '*Some more code*  
#loop  
```

#### With counter:  

```calcpad
#for counter = start : end  
  '*Your code goes here*  
#loop  
```

#### With condition:

```calcpad  
#while *condition*  
  '*Your code goes here*  
#loop  
```

### Modules and macros/string variables:  

#### Modules:  

  `#include filename` - include external file (module)  
  `#local` - start local section (not to be included)  
  `#global` - start global section (to be included)  
  
#### Inline string variable:  

  `#def variable_name$ = content`  
  
#### Multiline string variable:  

```calcpad
#def *variable_name$*  
  '*content line 1*  
  '*content line 2*  
  '...  
#end def  
```

#### Inline string macro:  

```calcpad
  #def macro_name$(param1$; param2$;...) = content
```

#### Multiline string macro:  

```calcpad
#def macro_name$(param1$; param2$;...)  
  '*content line 1*  
  '*content line 2*  
  '...  
#end def  
```
  
### Import/Export of external data:  

#### Text/CSV files:  

  `#read M from filename.txt@R1C1:R2C2 TYPE=R SEP=','` - read matrix M from a text/CSV file  
  `#write M to filename.txt@R1C1:R2C2 TYPE=N SEP=','` - write matrix M to a text/CSV file  
  `#append M to filename.txt@R1C1:R2C2 TYPE=N SEP=','` - append matrix M to a text/CSV file  
  
#### Excel files (xlsx and xlsm):  

  `#read M from filename.xlsx@Sheet1!A1:B2 TYPE=R` - read matrix M from an Excel file  
  `#write M to filename.xlsx@Sheet1!A1:B2 TYPE=N` - write matrix M to an Excel file  
  `#append M to filename.xlsx@Sheet1!A1:B2 TYPE=N` - append matrix M to an Excel file (same as write)  
  Sheet, range, TYPE and SEP can be omitted.  
  For #read command, TYPE can be either of \[R|D|C|S|U|L|V\].  
  For #write and #append commands, TYPE can be Y or N.  
	
### Output control:  

  `#hide` - hide the report contents  
  `#show` - always show the contents (default)  
  `#pre`  - show the next contents only before calculations  
  `#post` - show the next contents only after calculations  
  `#val`  - show only the final result, without the equation  
  `#equ`  - show complete equations and results (default)  
  `#noc`  - show only equations without results (no calculations)  
  `#nosub`  - do not substitute variables (no substitution)  
  `#novar`  - show equations only with substituted values (no variables)  
  `#varsub` - show equations with variables and substituted values (default)  
  `#split` - split equations that do not fit on a single line  
  `#wrap` - wrap equations that do not fit on a single line (default)  
  `#round n` - rounds the output to n digits after the decimal point  
  `#round default` - restores rounding to the default settings  
  `#format FFFF` - specifies custom format string  
  `#format default` - restores the default formatting  
  `#md on` - enables markdown in comments  
  `#md off` - disables markdown in comments  
  `#phasor` - sets output format of complex numbers to polar phasor: A∠φ  
  `#complex` - sets output format of complex numbers to Cartesian algebraic: a + bi.  
  Each of the above commands is effective after the current line until the end of the report or another command that overwrites it.  
	
### Breakpoints for step-by-step execution:  

  `#pause` - calculates to the current line and waits until resumed manually  
  `#input` - renders an input form to the current line and waits for user input.  
	
### Switches for trigonometric units:

  `#deg` - degrees
  `#rad` - radians
  `#gra` - gradians  

### Separator for target units: 

  `|`, for example:  `3ft + 12in|cm` will show 121.92 cm  

### Dimensionless: 

  `%`, `‰`, `‱`, `pcm`, `ppm`, `ppb`, `ppt`, `ppq`  

### Angle units: 

  `°`, `′`, `″`, `deg`, `rad`, `grad`, `rev`  

### Metric units (SI and compatible):  

  Mass:             `g`, `hg`, `kg`, `t`, `kt`, `Mt`, `Gt`, `dg`, `cg`, `mg`, `μg`, `Da`, `u`  
  Length:           `m`, `km`, `dm`, `cm`, `mm`, `μm`, `nm`, `pm`, `AU`, `ly`  
  Time:             `s`, `ms`, `μs`, `ns`, `ps`, `min`, `h`, `d`, `w`, `y`  
  Frequency:        `Hz`, `kHz`, `MHz`, `GHz`, `THz`, `mHz`, `μHz`, `nHz`, `pHz`, `rpm`  
  Speed:            `kmh`  
  Electric current: `A`, `kA`, `MA`, `GA`, `TA`, `mA`, `μA`, `nA`, `pA`  
  Temperature:      `°C`, `Δ°C`, `K`  
  Amount of substance: `mol`  
  Luminous intensity: `cd`  
  Area:             `a`, `daa`, `ha`  
  Volume:           `L`, `daL`, `hL`, `dL`, `cL`, `mL`, `μL`, `nL`, `pL`  
  Force:            `dyn N`, `daN`, `hN`, `kN`, `MN`, `GN`, `TN`, `gf`, `kgf`, `tf`  
  Moment:           `Nm`, `kNm`  
  Pressure:         `Pa`, `daPa`, `hPa`, `kPa`, `MPa`, `GPa`, `TPa`, `dPa`, `cPa`, `mPa`, `μPa`, `nPa`, `pPa`,  
                    `bar`, `mbar`, `μbar`, `atm`, `at`, `Torr`, `mmHg`  
  Viscosity:        `P`, `cP`, `St`, `cSt`  
  Energy work:      `J`, `kJ`, `MJ`, `GJ`, `TJ`, `mJ`, `μJ`, `nJ`, `pJ`, 
                    `Wh`, `kWh`, `MWh`, `GWh`, `TWh`, `mWh`, `μWh`, `nWh`, `pWh` ,  
                    `eV`, `keV`, `MeV`, `GeV`, `TeV`, `PeV`, `EeV`, `cal`, `kcal`, `erg  
  Power:            `W`, `kW`, `MW`, `GW`, `TW`, `mW`, `μW`, `nW`, `pW`, `hpM`, `ks  
                    `VA`, `kVA`, `MVA`, `GVA`, `TVA`, `mVA`, `μVA`, `nVA`, `pVA`,  
                    `VAR`, `kVAR`, `MVAR`, `GVAR`, `TVAR`, `mVAR`, `μVAR`, `nVAR`, `pVAR`, `hpM`, `ks` ,  
  Electric charge:  `C`, `kC`, `MC`, `GC`, `TC`, `mC`, `μC`, `nC`, `pC`, `Ah`, `mAh`  
  Potential:        `V`, `kV`, `MV`, `GV`, `TV`, `mV`, `μV`, `nV`, `pV`  
  Capacitance:      `F`, `kF`, `MF`, `GF`, `TF`, `mF`, `μF`, `nF`, `pF`  
  Resistance:       `Ω`, `kΩ`, `MΩ`, `GΩ`, `TΩ`, `mΩ`, `μΩ`, `nΩ`, `pΩ`  
  Conductance:      `S`, `kS`, `MS`, `GS`, `TS`, `mS`, `μS`, `nS`, `pS`, `℧`, `k℧`, `M℧`, `G℧`, `T℧`, `m℧`, `μ℧`, `n℧`, `p℧`  
  Magnetic flux:    `Wb `, `kWb`, `MWb`, `GWb`, `TWb`, `mWb`, `μWb`, `nWb`, `pWb`  
  Magnetic flux density: `T`, `kT`, `MT`, `GT`, `TT`, `mT`, `μT`, `nT`, `pT`  
  Inductance:       `H`, `kH`, `MH`, `GH`, `TH`, `mH`, `μH`, `nH`, `pH`  
  Luminous flux:    `lm`  
  Illuminance:      `lx`  
  Radioactivity:    `Bq`, `kBq`, `MBq`, `GBq`, `TBq`, `mBq`, `μBq`, `nBq`, `pBq`, `Ci`, `Rd`  
  Absorbed dose:    `Gy`, `kGy`, `MGy`, `GGy`, `TGy`, `mGy`, `μGy`, `nGy`, `pGy`  
  Equivalent dose:  `Sv`, `kSv`, `MSv`, `GSv`, `TSv`, `mSv`, `μSv`, `nSv`, `pSv`  
  Catalytic activity: `kat`  
	
### Non-metric units (Imperial/US):  

  Mass:             `gr`, `dr`, `oz`, `lb` (or `lbm`, `lb_m`), `klb`, `kipm` (or `kip_m`), `st`, `qr`,  
                    `cwt` (or `cwt_UK`, `cwt_US`), `ton` (or `ton_UK`, `ton_US`), `slug`  
  Length:           `th`, `in`, `ft`, `yd`, `ch`, `fur`, `mi`, `ftm (or ftm_UK`, `ftm_US)`,  
                    `cable` (or `cable_UK`, `cable_US`), `nmi`, `li`, `rod`, `pole`, `perch`, `lea`  
  Speed:            `mph`, `knot`  
  Temperature:      `°F`, `Δ°F`, `°R`  
  Area:             `rood`, `ac`  
  Volume, fluid:    `fl_oz`, `gi`, `pt`, `qt`, `gal`, `bbl`, or  
                    `fl_oz_UK`, `gi_UK`, `pt_UK`, `qt_UK`, `gal_UK`, `bbl_UK`,  
                    `fl_oz_US`, `gi_US`, `pt_US`, `qt_US`, `gal_US`, `bbl_US`,  
  Volume, dry: (US) `pt_dry`, (US) `qt_dry`, (US) `gal_dry`, (US) `bbl_dry`,  
                    `pk` (or `pk_UK`, `pk_US`), `bu` (or `bu_UK`, `bu_US`)  
  Force:            `ozf` (or `oz_f`), `lbf` (or `lb_f`), `kip` (or `kipf`, `kip_f`), `tonf` (or `ton_f`), `pdl`  
  Pressure:         `osi`, `osf psi`, `psf`, `ksi`, `ksf`, `tsi`, `tsf`, `inHg`  
  Energy/work:      `BTU`, `therm`, `(or therm_UK`, `therm_US)`, `quad`  
  Power:            `hp`, `hpE`, `hpS`  
	
### Custom units: 

  `.Name = expression`
 
Unit names can include currency symbols: `€`, `£`, `₤`, `¥`, `¢`, `₽`, `₹`, `₩`, `₪'

---

## ADDITIONAL NOTES ABOUT MATRICES AND VECTORS

1. Individual vector elements are accessed by the indexing `.` "dot" operator. 
   For example: `a.1 = 5`, `a_k = a.k`, `a.(k + 1) = 6`.
2. Matrix elements are accessed by the indexing operator followed by two indexes in brackets: 
   For example: `A.(1; 1) = 5`, `A_ij = A.(i; j)`, `A.(i + 1; j + 2) = 6`.
3. Calcpad automatically maintains the structure of special matrices (diagonal, triangular, symmetric, column).
4. Assigning a value to the off-diagonal or off-triangle element throws an error.
5. Reading a value from the off-diagonal or off-triangle element returns zero.
6. Matrices created by `_hp` suffixed functions or the `hp()` function are of "hp" type. 
7. All elements in an "hp" vector or matrix must have the same or consistent units (or no units at all).
8. The other non-"hp" types can contain different units, but the should be consistent for all operations you perform on them. For example, you cannot sum elements with inconsistent units.
9. "Hp" types are much faster and more memory efficient. Always prefer "hp" types when there are no units or units are equal/consistent or the vectors/matrices are larger than 1000 even if you will have to remove the units before that and restore them later.


## COMMON MISTAKES TO AVOID

1. Assignment vs. equality
   - Use `=` for assignment: `a = 5`
   - Use `≡` or `==` for comparison: `#if a ≡ 5`

2. Semicolons as delimiters in functions
   - Parameters separated by `;` not `,`
   - Correct: `f(x; y; z)`
   - Wrong: `f(x, y, z)`

3. Line continuation
   - Use ` _` at end of line for continuation
   - Or end line with opening bracket or delimiter

4. Comments required for multiple expressions on a line
   ```calcpad
   a = 5'comment'b = 10    'Correct'
   a = 5 b = 10            'Wrong - syntax error'
   ```

5. Angle units
   - Set `#deg`, `#rad`, or `#gra` before  using trigonometric functions. The default angle units are `#deg`
   - Or attach units directly: `sin(45*deg)`
   - Angle units, including `rad` are not consistent with dimensionless values and `rpm` units. You will have to remove them manually in such cases by dividing to them.

---

## QUICK REFERENCE EXAMPLES

### Simple Calculation
```calcpad
'Beam Moment Calculation'
'Span -'L = 6m             
'Load -'q = 10kN/m         
'Moment -'M = w*L^2/8         
```

### With Function
```calcpad
stress(P; A) = P/A
P = 100kN
A = 0.01m^2
σ = stress(P; A) | MPa
```

### With Conditional
```calcpad
#if σ > 250*MPa
  'The check is NOT satisfied ✖'
#else
  'heck is satisfied ✔'
#end if
```

### With Loop
```calcpad
'Sum of squares'
sum = 0
#for i = 1 : 10
  sum = sum + i^2
#loop
```

### With Plot
```calcpad
'Visualize function'
$Plot{x^2 - 4*x + 3 @ x = 0 : 5}
```

### With Numerical Method
```calcpad
'Find root of equation'
x = $Root{x^2 - 10 @ x = 0 : 5}
'Result: 'x' ≈ 3.162'
```

---

## SYNTAX SUMMARY

| Element  | Syntax                | Example                  |
|----------|-----------------------|--------------------------|
| Variable | `name = value`        | `x = 5`                  |
| Function | `f(params) = expr`    | `f(x) = x^2`             |
| Comment  | `'text'` or `"title"` | `'Length'`               |
| If       | `#if ... #end if`     | `#if x > 0 ... #end if`  |
| For loop | `#for i = a : b`      | `#for i = 1 : 10`        |
| Include  | `#include file.cpd`   | `#include materials.cpd` |
| Plot     | `$Plot{f(x) @ x=a:b}` | `$Plot{sin(x) @ x=0:π}`  |
| Root     | `$Root{f(x) @ x=a:b}` | `$Root{x^2-5 @ x=0:10}`  |
| Integral | `$Area{f(x) @ x=a:b}` | `$Area{sin(x) @ x=0:π}`  |

---

## RESOURCES

- Official Site: https://calcpad.eu/
- GitHub: https://github.com/Proektsoftbg/Calcpad
- Documentation: https://calcpad.eu/help/
- Examples: https://github.com/Proektsoftbg/Calcpad/tree/main/Examples
- Blog: https://calcpad.blog/

---

## FILE EXTENSION
- Use `.cpd` for Calcpad source files.

## EXECUTION
- Press Ctrl+Shift+B to run in VS Code or the command `Calcpad: Run Calcpad File`. 

---
