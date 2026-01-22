import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { exec } from 'child_process';
import { spawn } from "child_process";
import * as https from 'https';
let currentPanel: vscode.WebviewPanel | undefined = undefined;
let agentPanel: vscode.WebviewPanel | undefined = undefined;
const completionItems: string[] = [
    // Keywords
    "#append", "#break", "#complex", "#continue", "#def", "#deg", "#else if", "#else",
    "#end def", "#end if", "#equ", "#for", "#while", "#global", "#gra", "#hide", "#if",
    "#include", "#input", "#local", "#loop", "#md", "#md off", "#md on", "#noc", "#nosub",
    "#novar", "#pause", "#phasor", "#post", "#pre", "#rad", "#read", "#repeat", "#round",
    "#show", "#split", "#val", "#varsub", "#wrap", "#write",
    
    // Methods
    "$Area", "$Block", "$Derivative", "$Find", "$Inf", "$Inline", "$Integral", "$Map",
    "$Plot", "$Product", "$Repeat", "$Root", "$Slope", "$Sum", "$Sup", "$While",
    
    // Units and constants
    "A", "AU", "Ah", "BTU", "Bq", "C", "Ci", "Da", "EeV", "F", "GA", "GBq", "GC", "GF",
    "GGy", "GH", "GHz", "GJ", "GN", "GPa", "GS", "GSv", "GT", "GV", "GVA", "GVAR", "GW",
    "GWb", "GWh", "GeV", "Gt", "Gy", "GΩ", "G℧", "H", "Hz", "J", "K", "L", "MA", "MBq",
    "MC", "MF", "MGy", "MH", "MHz", "MJ", "MN", "MPa", "MS", "MSv", "MT", "MV", "MVA",
    "MVAR", "MW", "MWb", "MWh", "MeV", "Mt", "MΩ", "M℧", "N", "Nm", "P", "Pa", "PeV",
    "Rd", "S", "St", "Sv", "T", "TA", "TBq", "TC", "TF", "TGy", "TH", "THz", "TJ", "TN",
    "TPa", "TS", "TSv", "TT", "TV", "TVA", "TVAR", "TW", "TWb", "TWh", "TeV", "Torr",
    "TΩ", "T℧", "V", "VA", "VAR", "W", "Wb", "Wh",
    
    // Settings
    "PlotAdaptive", "PlotHeight", "PlotLightDir", "PlotPalette", "PlotShadows",
    "PlotSmooth", "PlotStep", "PlotSVG", "PlotWidth", "Precision", "ReturnAngleUnits", "Tol",
    
    // Functions
    "abs", "acos", "acosh", "acot", "acoth", "acsc", "acsch", "add", "adj", "and", "asec",
    "asech", "asin", "asinh", "atan", "atan2", "atanh", "augment", "average", "cbrt",
    "ceiling", "cholesky", "clrunits", "clsolve", "cmsolve", "cofactor", "col", "column",
    "column_hp", "cond", "cond_1", "cond_2", "cond_e", "cond_i", "conj", "copy", "cos",
    "cosh", "cot", "coth", "count", "cross", "csc", "csch", "det", "diag2vec", "diagonal",
    "diagonal_hp", "dot", "eigen", "eigenvals", "eigenvecs", "exp", "extract",
    "extract_cols", "extract_rows", "fill", "fill_col", "fill_row", "find", "find_eq",
    "find_ge", "find_gt", "find_le", "find_lt", "find_ne", "first", "floor", "fprod",
    "gcd", "getunits", "hlookup", "hlookup_eq", "hlookup_ge", "hlookup_gt", "hlookup_le",
    "hlookup_lt", "hlookup_ne", "hp", "hprod", "identity", "identity_hp", "if", "im",
    "inverse", "ishp", "join", "join_cols", "join_rows", "kprod", "last", "lcm", "len",
    "line", "ln", "log", "log_2", "lookup", "lookup_eq", "lookup_ge", "lookup_gt", 
    "lookup_le", "lookup_lt", "lookup_ne", "lsolve", "ltriang", "ltriang_hp", "lu",
    "matmul", "matrix", "matrix_hp", "max", "mcount", "mean", "mfill", "mfind", "mfind_eq",
    "mfind_ge", "mfind_gt", "mfind_le", "mfind_lt", "mfind_ne", "min", "mnorm", "mnorm_1",
    "mnorm_2", "mnorm_e", "mnorm_i", "mod", "mresize", "msearch", "msolve", "n_cols",
    "n_rows", "norm", "norm_1", "norm_2", "norm_e", "norm_i", "norm_p", "not", "or",
    "order", "order_cols", "order_rows", "phase", "product", "qr", "random", "range",
    "range_hp", "rank", "re", "resize", "reverse", "revorder", "revorder_cols",
    "revorder_rows", "root", "round", "row", "rsort", "rsort_cols", "rsort_rows", "search",
    "sec", "sech", "setunits", "sign", "sin", "sinh", "size", "slice", "slsolve", "smsolve",
    "sort", "sort_cols", "sort_rows", "spline", "sqr", "sqrt", "srss", "stack", "submatrix",
    "sum", "sumsq", "svd", "switch", "symmetric", "symmetric_hp", "take", "tan", "tanh",
    "timer", "trace", "transp", "trunc", "unit", "utriang", "utriang_hp", "vec2col",
    "vec2diag", "vec2row", "vector", "vector_hp", "vlookup", "vlookup_eq", "vlookup_ge",
    "vlookup_gt", "vlookup_le", "vlookup_lt", "vlookup_ne", "xor",
    
    // Units
    "a", "d", "h", "min", "ms", "ns", "ps", "s", "ks", "y",
    "ac", "at", "atm", "bar", "bbl", "bbl_UK", "bbl_US", "bbl_dry", "bu", "bu_UK", "bu_US",
    "cL", "cP", "cPa", "cSt", "cable", "cable_UK", "cable_US", "cal", "cd", "cg", "ch",
    "cm", "cwt", "cwt_UK", "cwt_US", "dL", "dPa", "daL", "daN", "daPa", "daa", "deg", "dg",
    "dm", "dr", "dyn", "eV", "erg", "fl_oz", "fl_oz_UK", "fl_oz_US", "ft", "ftm", "ftm_UK",
    "ftm_US", "fur", "g", "gal", "gal_UK", "gal_US", "gal_dry", "gf", "gi", "gi_UK",
    "gi_US", "gr", "grad", "hL", "hN", "hPa", "ha", "hg", "hpE", "hpM", "hpS", "in",
    "inHg", "kA", "kBq", "kC", "kF", "kGy", "kH", "kHz", "kJ", "kN", "kNm", "kPa", "kS",
    "kSv", "kT", "kV", "kVA", "kVAR", "kW", "kWb", "kWh", "kat", "kcal", "keV", "kg",
    "kgf", "kip", "kip_f", "kip_m", "kipf", "kipm", "km", "kmh", "knot", "ksf", "ksi",
    "kt", "kΩ", "k℧", "lb", "lb_f", "lb_m", "lbf", "lbm", "lea", "li", "lm", "lx", "ly",
    "m", "mA", "mAh", "mBq", "mC", "mF", "mGy", "mH", "mHz", "mJ", "mL", "mPa", "mS",
    "mSv", "mT", "mV", "mVA", "mVAR", "mW", "mWb", "mWh", "mbar", "mg", "mi", "mm",
    "mmHg", "mol", "mph", "mΩ", "m℧", "nA", "nBq", "nC", "nF", "nGy", "nH", "nHz", "nJ",
    "nL", "nPa", "nS", "nSv", "nT", "nV", "nVA", "nVAR", "nW", "nWb", "nWh", "ng", "nm",
    "nmi", "nΩ", "n℧", "osf", "osi", "oz", "oz_f", "ozf", "pA", "pBq", "pC", "pF", "pGy",
    "pH", "pHz", "pJ", "pL", "pPa", "pS", "pSv", "pT", "pV", "pVA", "pVAR", "pW", "pWb",
    "pWh", "pdl", "perch", "pg", "pk", "pk_UK", "pk_US", "pm", "pole", "psf", "psi", "pt",
    "pt_UK", "pt_US", "pt_dry", "pΩ", "p℧", "qt", "qt_UK", "qt_US", "qt_dry", "quad",
    "rad", "rev", "rod", "rood", "rpm", "slug", "st", "t", "tf", "th", "therm", "therm_UK",
    "therm_US", "ton", "ton_UK", "ton_US", "ton_f", "tonf", "tsf", "tsi", "u", "w", "yd",
    
    // Special units
    "°C", "°F", "°R", "Δ°C", "Δ°F", "Ω", "μA", "μBq", "μC", "μF", "μGy", "μH", "μHz",
    "μJ", "μL", "μPa", "μS", "μSv", "μT", "μV", "μVA", "μVAR", "μW", "μWb", "μWh",
    "μbar", "μg", "μm", "μs", "μΩ", "μ℧", "℧"
];

// Function descriptions for hover information
// Function descriptions for hover information
const functionDescriptions: { [key: string]: string } = {
    // Trigonometric functions
    "sin": "Sine function:\n\n    sin(x)",
    "cos": "Cosine function:\n\n    cos(x)",
    "tan": "Tangent function:\n\n    tan(x)",
    "csc": "Cosecant function:\n\n    csc(x)",
    "sec": "Secant function:\n\n    sec(x)",
    "cot": "Cotangent function:\n\n    cot(x)",
    
    // Hyperbolic functions
    "sinh": "Hyperbolic sine:\n\n    sinh(x)",
    "cosh": "Hyperbolic cosine:\n\n    cosh(x)",
    "tanh": "Hyperbolic tangent:\n\n    tanh(x)",
    "csch": "Hyperbolic cosecant:\n\n    csch(x)",
    "sech": "Hyperbolic secant:\n\n    sech(x)",
    "coth": "Hyperbolic cotangent:\n\n    coth(x)",
    
    // Inverse trigonometric functions
    "asin": "Inverse sine (arcsine):\n\n    asin(x)",
    "acos": "Inverse cosine (arccosine):\n\n    acos(x)",
    "atan": "Inverse tangent (arctangent):\n\n    atan(x)",
    "atan2": "The angle whose tangent is the quotient of y and x:\n\n    atan2(y; x)",
    "acsc": "Inverse cosecant:\n\n    acsc(x)",
    "asec": "Inverse secant:\n\n    asec(x)",
    "acot": "Inverse cotangent:\n\n    acot(x)",
    
    // Inverse hyperbolic functions
    "asinh": "Inverse hyperbolic sine:\n\n    asinh(x)",
    "acosh": "Inverse hyperbolic cosine:\n\n    acosh(x)",
    "atanh": "Inverse hyperbolic tangent:\n\n    atanh(x)",
    "acsch": "Inverse hyperbolic cosecant:\n\n    acsch(x)",
    "asech": "Inverse hyperbolic secant:\n\n    asech(x)",
    "acoth": "Inverse hyperbolic cotangent:\n\n    acoth(x)",
    
    // Logarithmic, exponential and roots
    "log": "Decimal (base-10) logarithm:\n\n    log(x)",
    "ln": "Natural logarithm:\n\n    ln(x)",
    "log_2": "Binary (base-2) logarithm:\n\n    log_2(x)",
    "exp": "Natural exponent (e^x):\n\n    exp(x)",
    "sqr": "Square root:\n\n    sqr(x)",
    "sqrt": "Square root:\n\n    sqrt(x)",
    "cbrt": "Cubic root:\n\n    cbrt(x)",
    "root": "N-th root:\n\n    root(x; n)",
    
    // Rounding functions
    "round": "Rounds to the nearest integer:\n\n    round(x)",
    "floor": "Rounds down to the smaller integer (towards -∞):\n\n    floor(x)",
    "ceiling": "Rounds up to the greater integer (towards +∞):\n\n    ceiling(x)",
    "trunc": "Rounds to the smaller integer (towards zero):\n\n    trunc(x)",
    
    // Integer functions
    "mod": "Returns the remainder of an integer division:\n\n    mod(x; y)",
    "gcd": "Greatest common divisor of several integers:\n\n    gcd(x; y; z...)",
    "lcm": "Least common multiple of several integers:\n\n    lcm(x; y; z...)",
    
    // Complex number functions
    "re": "Returns the real part of a complex number:\n\n    re(z)",
    "im": "Returns the imaginary part of a complex number:\n\n    im(z)",
    "abs": "Returns the absolute value/magnitude:\n\n    abs(z)",
    "phase": "Returns the phase angle of a complex number:\n\n   phase(z)",
    "conj": "Returns the complex conjugate:\n\n    conj(z)",
    
    // Aggregate and interpolation functions
    "min": "Minimum of multiple values:\n\n    min(x; y; z...)",
    "max": "Maximum of multiple values:\n\n    max(x; y; z...)",
    "sum": "Sum of multiple values:\n\n    sum(x; y; z...) = x + y + z...",
    "sumsq": "Sum of squares:\n\n    sumsq(x; y; z...) = x² + y² + z²...",
    "srss": "Square root of sum of squares:\n\n    srss(x; y; z...) = sqrt(x² + y² + z²...)",
    "average": "Average of multiple values:\n\n    average(x; y; z...) = (x + y + z...)/n",
    "product": "Product of multiple values:\n\n    product(x; y; z...) = x·y·z...",
    "mean": "Geometric mean:\n\n    mean(x; y; z...) = n-th root(x·y·z...)",
    "take": "Returns the n-th element from a list or matrix element at indexes:\n\n    take(n; a; b; c...) or take(x; y; M)",
    "line": "Linear interpolation:\n\n    line(x; a; b; c...)\n    or double linear for matrices:\n    line(x; y; M)",
    "spline": "Hermite spline interpolation:\n\n    spline(x; a; b; c...) or double spline for matrices:\n    spline(x; y; M)",
    
    // Conditional and logical functions
    "if": "Conditional evaluation:\n\n    if(condition; value-if-true; value-if-false)",
    "switch": "Selective evaluation:\n\n    switch(cond1; value1; cond2; value2; ...; default)",
    "not": "Logical NOT:\n\n    not(x)",
    "and": "Logical AND:\n\n    and(x; y; z...)",
    "or": "Logical OR:\n\n    or(x; y; z...)",
    "xor": "Logical XOR:\n\n    xor(x; y; z...)",
    
    // Other utility functions
    "sign": "Returns the sign of a number (-1, 0, or 1):\n\n    sign(x)",
    "random": "Returns a random number between 0 and x:\n\n    random(x)",
    "timer": "Returns elapsed time:\n\n    timer(x)",
    "getunits": "Gets the units of x without the value. Returns 1 if x is unitless:\n\n    getunits(x)",
    "setunits": "Sets the units u to x where x can be scalar, vector or matrix:\n\n    setunits(x; u)",
    "clrunits": "Clears the units from a scalar, vector or matrix x:\n\n    clrunits(x)",
    "hp": "Converts x to its high performance (hp) equivalent type:\n\n    hp(x)",
    "ishp": "Checks if the type of x is a high-performance (hp) vector or matrix:\n\n    ishp(x)",
    
    // Vector - Creational
    "vector": "Creates an empty vector with length n:\n\n    vector(n)",
    "vector_hp": "Creates an empty high performance (hp) vector with length n:\n\n    vector_hp(n)",
    "range": "Creates a vector with values spanning from x1 to xn with step s:\n\n    range(x1; xn; s)",
    "range_hp": "Creates a high performance (hp) vector from a range of values:\n\n    range_hp(x1; xn; s)",
    
    // Vector - Structural
    "len": "Returns the length of vector v:\n\n    len(v)",
    "size": "Returns the actual size of vector v:\n\n    size(v) - the index of the last non-zero element",
    "resize": "Sets a new length n of vector v:\n\n    resize(v; n)",
    "fill": "Fills vector v with value x:\n\n    fill(v; x)",
    "join": "Creates a vector by joining arguments - matrices, vectors and scalars:\n\n    join(A; b; c...)",
    "slice": "Returns the part of vector v bounded by indexes i₁ and i₂ inclusive:\n\n    slice(v; i₁; i₂)",
    "first": "Returns the first n elements of vector v:\n\n    first(v; n)",
    "last": "Returns the last n elements of vector v:\n\n    last(v; n)",
    "extract": "Extracts the elements from v which indexes are contained in i:\n\n    extract(v; i)",
    
    // Vector - Data
    "sort": "Sorts the elements of vector v in ascending order:\n\n    sort(v)",
    "rsort": "Sorts the elements of vector v in descending order:\n\n    rsort(v)",
    "order": "Returns the indexes of vector v, arranged by the ascending order of its elements:\n\n    order(v)",
    "revorder": "Returns the indexes of vector v, arranged by the descending order of its elements:\n\n    revorder(v)",
    "reverse": "Returns a new vector containing the elements of v in reverse order:\n\n    reverse(v)",
    "count": "Returns the number of elements in v, after the i-th one, that are equal to x:\n\n    count(v; x; i)",
    "search": "Returns the index of the first element in v, after the i-th one, that is equal to x:\n\n    search(v; x; i)",
    "find": "Returns the indexes of all elements in v, after the i-th one, that are = x:\n\n    find(v; x; i)",
    "find_eq": "Returns the indexes of all elements in v, after the i-th one, that are = x:\n\n    find_eq(v; x; i)",
    "find_ne": "Returns the indexes of all elements in v, after the i-th one, that are ≠ x:\n\n    find_ne(v; x; i)",
    "find_lt": "Returns the indexes of all elements in v, after the i-th one, that are < x:\n\n    find_lt(v; x; i)",
    "find_le": "Returns the indexes of all elements in v, after the i-th one, that are ≤ x:\n\n    find_le(v; x; i)",
    "find_gt": "Returns the indexes of all elements in v, after the i-th one, that are > x:\n\n    find_gt(v; x; i)",
    "find_ge": "Returns the indexes of all elements in v, after the i-th one, that are ≥ x:\n\n    find_ge(v; x; i)",
    "lookup": "Returns all elements in a for which the respective elements in b are = x:\n\n    lookup(a; b; x)",
    "lookup_eq": "Returns all elements in a for which the respective elements in b are = x:\n\n    lookup_eq(a; b; x)",
    "lookup_ne": "Returns all elements in a for which the respective elements in b are ≠ x:\n\n    lookup_ne(a; b; x)",
    "lookup_lt": "Returns all elements in a for which the respective elements in b are < x:\n\n    lookup_lt(a; b; x)",
    "lookup_le": "Returns all elements in a for which the respective elements in b are ≤ x:\n\n    lookup_le(a; b; x)",
    "lookup_gt": "Returns all elements in a for which the respective elements in b are > x:\n\n    lookup_gt(a; b; x)",
    "lookup_ge": "Returns all elements in a for which the respective elements in b are ≥ x:\n\n    lookup_ge(a; b; x)",
    
    // Vector - Math
    "norm_1": "L1 (Manhattan) norm of vector v:\n\n    norm_1(v)",
    "norm": "L2 (Euclidean) norm of vector v:\n\n    norm(v)",
    "norm_2": "L2 (Euclidean) norm of vector v:\n\n    norm_2(v)",
    "norm_e": "L2 (Euclidean) norm of vector v:\n\n    norm_e(v)",
    "norm_p": "Lp norm of vector v:\n\n    norm_p(v; p)",
    "norm_i": "L∞ (infinity) norm of vector v:\n\n    norm_iv)",
    "unit": "Returns the normalized vector v (with L2 norm = 1):\n\n    unit(v)",
    "dot": "Scalar product of two vectors a and b:\n\n    dot(a; b)",
    "cross": "Cross product of two vectors a and b (with length 2 or 3):\n\n    cross(a; b)",
    
    // Matrix - Creational
    "matrix": "Creates an empty matrix with dimensions m×n:\n\n    matrix(m; n)",
    "identity": "Creates an identity matrix with dimensions n×n:\n\n    identity(n)",
    "diagonal": "Creates a n×n diagonal matrix and fills the diagonal with value d:\n\n    diagonal(n; d)",
    "column": "Creates a column matrix with dimensions m×1, filled with value c:\n\n    column(m; c)",
    "utriang": "Creates an upper triangular matrix with dimensions n×n:\n\n    utriang(n)",
    "ltriang": "Creates a lower triangular matrix with dimensions n×n:\n\n    ltriang(n)",
    "symmetric": "Creates a symmetric matrix with dimensions n×n:\n\n    symmetric(n)",
    "matrix_hp": "Creates a high-performance matrix with dimensions m×n:\n\n    matrix_hp(m; n)",
    "identity_hp": "Creates a high-performance identity matrix with dimensions n×n:\n\n    identity_hp(n)",
    "diagonal_hp": "Creates a high-performance n×n diagonal matrix filled with value d:\n\n    diagonal_hp(n; d)",
    "column_hp": "Creates a high-performance m×1 column matrix filled with value c:\n\n    column_hp(m; c)",
    "utriang_hp": "Creates a high-performance n×n upper triangular matrix:\n\n    utriang_hp(n)",
    "ltriang_hp": "Creates a high-performance n×n lower triangular matrix:\n\n    ltriang_hp(n)",
    "symmetric_hp": "Creates a high-performance symmetric matrix with dimensions n×n:\n\n    symmetric_hp(n)",
    "vec2diag": "Creates a diagonal matrix from the elements of vector v:\n\n    ",
    "vec2row": "Creates a row matrix from the elements of vector v:\n\n    vec2row(v)",
    "vec2col": "Creates a column matrix from the elements of vector v:\n\n    vec2col(v)",
    "join_cols": "Creates a new matrix by joining column vectors:\n\n    join_cols(c₁; c₂; c₃...)",
    "join_rows": "Creates a new matrix by joining row vectors:\n\n    join_rows(r₁; r₂; r₃...)",
    "augment": "Creates a new matrix by appending matrices A; B; C side by side:\n\n    augment(A; B; C...)",
    "stack": "Creates a new matrix by stacking matrices A; B; C one below the other:\n\n    stack(A; B; C...)",
    
    // Matrix - Structural
    "n_rows": "Returns the number of rows in matrix M:\n\n    n_rows(M)",
    "n_cols": "Returns the number of columns in matrix M:\n\n    n_cols(M)",
    "mresize": "Sets new dimensions m and n for matrix M:\n\n    mresize(M; m; n)",
    "mfill": "Fills matrix M with value x:\n\n    mfill(M; x)",
    "fill_row": "Fills the i-th row of matrix M with value x:\n\n    fill_row(M; i; x)",
    "fill_col": "Fills the j-th column of matrix M with value x:\n\n    fill_col(M; j; x)",
    "copy": "Copies all elements from A to B, starting from indexes i and j of B:\n\n    copy(A; B; i; j)",
    "add": "Adds all elements from A to those of B, starting from indexes i and j of B:\n\n    add(A; B; i; j)",
    "row": "Extracts the i-th row of matrix M as a vector:\n\n    row(M; i)",
    "col": "Extracts the j-th column of matrix M as a vector:\n\n    col(M; j)",
    "extract_rows": "Extracts the rows from matrix M whose indexes are contained in vector i:\n\n    extract_rows(M; i)",
    "extract_cols": "Extracts the columns from matrix M whose indexes are contained in vector j:\n\n    extract_cols(M; j)",
    "diag2vec": "Extracts the diagonal elements of matrix M to a vector:\n\n    diag2vec(M)",
    "submatrix": "Extracts a submatrix of M, bounded by rows i₁ and i₂ and columns j₁ and j₂:\n\n    submatrix(M; i₁; i₂; j₁; j₂)",
    
    // Matrix - Data
    "sort_cols": "Sorts the columns of M based on the values in row i in ascending order:\n\n    sort_cols(M; i)",
    "rsort_cols": "Sorts the columns of M based on the values in row i in descending order:\n\n    rsort_cols(M; i)",
    "sort_rows": "Sorts the rows of M based on the values in column j in ascending order:\n\n    sort_rows(M; j)",
    "rsort_rows": "Sorts the rows of M based on the values in column j in descending order:\n\n    rsort_rows(M; j)",
    "order_cols": "Returns the indexes of the columns of M based on the ordering of the values from row i in ascending order:\n\n    order_cols(M; i)",
    "revorder_cols": "Returns the indexes of the columns of M based on the ordering of the values from row i in descending order:\n\n    revorder_cols(M; i)",
    "order_rows": "Returns the indexes of the rows of M based on the ordering of the values in column j in ascending order:\n\n    order_rows(M; j)",
    "revorder_rows": "Returns the indexes of the rows of M based on the ordering of the values in column j in descending order:\n\n    revorder_rows(M; j)",
    "mcount": "Returns the number of occurrences of value x in matrix M:\n\n    mcount(M; x)",
    "msearch": "Returns a vector with the two indexes of the first occurrence of x in matrix M, starting from indexes i and j:\n\n    msearch(M; x; i; j)",
    "mfind": "Returns the indexes of all elements in M that are = x:\n\n    mfind(M; x)",
    "mfind_eq": "Returns the indexes of all elements in M that are = x:\n\n    mfind_eq(M; x)",
    "mfind_ne": "Returns the indexes of all elements in M that are ≠ x:\n\n    mfind_ne(M; x)",
    "mfind_lt": "Returns the indexes of all elements in M that are < x:\n\n    mfind_lt(M; x)",
    "mfind_le": "Returns the indexes of all elements in M that are ≤ x:\n\n    mfind_le(M; x)",
    "mfind_gt": "Returns the indexes of all elements in M that are > x:\n\n    mfind_gt(M; x)",
    "mfind_ge": "Returns the indexes of all elements in M that are ≥ x:\n\n    mfind_ge(M; x)",
    "hlookup": "Returns the values from row i₂ of M, for which the elements in row i₁ are = x:\n\n    hlookup(M; x; i₁; i₂)",
    "hlookup_eq": "Returns the values from row i₂ of M, for which the elements in row i₁ are = x:\n\n    hlookup_eq(M; x; i₁; i₂)",
    "hlookup_ne": "Returns the values from row i₂ of M, for which the elements in row i₁ are ≠ x:\n\n    hlookup_ne(M; x; i₁; i₂)",
    "hlookup_lt": "Returns the values from row i₂ of M, for which the elements in row i₁ are < x:\n\n    hlookup_lt(M; x; i₁; i₂)",
    "hlookup_le": "Returns the values from row i₂ of M, for which the elements in row i₁ are ≤ x:\n\n    hlookup_le(M; x; i₁; i₂)",
    "hlookup_gt": "Returns the values from row i₂ of M, for which the elements in row i₁ are > x:\n\n    hlookup_gt(M; x; i₁; i₂)",
    "hlookup_ge": "Returns the values from row i₂ of M, for which the elements in row i₁ are ≥ x:\n\n    hlookup_ge(M; x; i₁; i₂)",
    "vlookup": "Returns the values from column j₂ of M, for which the elements in column j₁ are = x:\n\n    vlookup(M; x; j₁; j₂)",
    "vlookup_eq": "Returns the values from column j₂ of M, for which the elements in column j₁ are = x:\n\n    vlookup_eq(M; x; j₁; j₂)",
    "vlookup_ne": "Returns the values from column j₂ of M, for which the elements in column j₁ are ≠ x:\n\n    vlookup_ne(M; x; j₁; j₂)",
    "vlookup_lt": "Returns the values from column j₂ of M, for which the elements in column j₁ are < x:\n\n    vlookup_lt(M; x; j₁; j₂)",
    "vlookup_le": "Returns the values from column j₂ of M, for which the elements in column j₁ are ≤ x:\n\n    vlookup_le(M; x; j₁; j₂)",
    "vlookup_gt": "Returns the values from column j₂ of M, for which the elements in column j₁ are > x:\n\n    vlookup_gt(M; x; j₁; j₂)",
    "vlookup_ge": "Returns the values from column j₂ of M, for which the elements in column j₁ are ≥ x:\n\n    vlookup_ge(M; x; j₁; j₂)",
    
    // Matrix - Math
    "hprod": "Hadamard product of matrices A and B:\n\n    hprod(A; B)",
    "fprod": "Frobenius product of matrices A and B:\n\n    fprod(A; B)",
    "kprod": "Kronecker product of matrices A and B:\n\n    kprod(A; B)",
    "mnorm_1": "L1 norm of matrix M:\n\n    mnorm_1(M)",
    "mnorm": "L2 norm of matrix M:\n\n    mnorm(M)",
    "mnorm_2": "L2 norm of matrix M:\n\n    mnorm_2(M)",
    "mnorm_e": "Frobenius norm of matrix M:\n\n    mnorm_e(M)",
    "mnorm_i": "L∞ norm of matrix M:\n\n    mnorm_i(M)",
    "cond_1": "Condition number of M based on the L1 norm:\n\n    cond_1(M)",
    "cond": "Condition number of M based on the L2 norm:\n\n    cond(M)",
    "cond_2": "Condition number of M based on the L2 norm:\n\n    cond_2(M)",
    "cond_e": "Condition number of M based on the Frobenius norm:\n\n    cond_e(M)",
    "cond_i": "Condition number of M based on the L∞ norm:\n\n    cond_i(M)",
    "det": "Determinant of matrix M:\n\n    det(M)",
    "rank": "Rank of matrix M:\n\n    rank(M)",
    "trace": "Trace of matrix M (sum of diagonal elements):\n\n    trace(M)",
    "transp": "Transpose of matrix M:\n\n    transp(M)",
    "adj": "Adjugate of matrix M:\n\n    adj(M)",
    "cofactor": "Cofactor matrix of M:\n\n    cofactor(M)",
    "eigenvals": "Returns the first n_e eigenvalues of matrix M (or all if omitted):\n\n    eigenvals(M; n_e)",
    "eigenvecs": "Returns the first n_e eigenvectors of matrix M (or all if omitted):\n\n    eigenvecs(M; n_e)",
    "eigen": "Returns the first n_e eigenvalues and eigenvectors of M (or all if omitted):\n\n    eigen(M; n_e)",
    "cholesky": "Cholesky decomposition of a symmetric, positive-definite matrix M:\n\n    cholesky(M)",
    "lu": "LU decomposition of matrix M:\n\n    lu(M)",
    "qr": "QR decomposition of matrix M:\n\n    qr(M)",
    "svd": "Singular value decomposition of M:\n\n    svd(M)",
    "inverse": "Inverse of matrix M:\n\n    inverse(M)",
    "lsolve": "Solves the system of linear equations Ax = b using LDLT decomposition for symmetric matrices, and LU for non-symmetric:\n\n    lsolve(A; b)",
    "clsolve": "Solves the linear matrix equation Ax = b with symmetric, positive-definite matrix A using Cholesky decomposition:\n\n    clsolve(A; b)",
    "slsolve": "Solves the linear matrix equation Ax = b with high-performance symmetric, positive-definite matrix A using preconditioned conjugate gradient (PCG) method:\n\n    slsolve(A; b)",
    "msolve": "Solves the generalized matrix equation AX = B using LDLT decomposition for symmetric matrices, and LU for non-symmetric:\n\n    msolve(A; B)",
    "cmsolve": "Solves the generalized matrix equation AX = B with symmetric, positive-definite matrix A using Cholesky decomposition:\n\n    cmsolve(A; B)",
    "smsolve": "Solves the generalized matrix equation AX = B with high-performance symmetric, positive-definite matrix A using PCG method:\n\n    smsolve(A; B)",
    "matmul": "Fast multiplication of square hp matrices using parallel Winograd algorithm:\n\n    matmul(A; B)",
    "fft": "Performs fast Fourier transform of row-major matrix M. It must have one row for real data and two rows for complex:\n\n    fft(M)",
    "ift": "Performs inverse Fourier transform of row-major matrix M. It must have one row for real data and two rows for complex:\n\n    ift(M)"
};

// Keyword descriptions for hover information
const keywordDescriptions: { [key: string]: string } = {    
    // Keywords
    "#append": "Append matrix M to a text/CSV or Excel file:\n\n    #append M to filename.txt@R1C1:R2C2 TYPE=N SEP=','",
    "#break": "Breaks out of the current iteration loop",
    "#complex": "Sets output format of complex numbers to Cartesian algebraic: a + bi",
    "#continue": "Continues to the next iteration of the loop",
    "#def": "Defines a string variable or macro:\n\n    #def variable$ = content",
    "#deg": "Sets trigonometric units to degrees",
    "#else if": "Alternative condition in conditional block",
    "#else": "Alternative branch in conditional block",
    "#end def": "Ends a multi-line string variable or macro definition",
    "#end if": "Ends a conditional block",
    "#equ": "Shows complete equations and results (default)",
    "#for": "Iteration loop with counter:\n\n    #for counter = start : end",
    "#while": "Iteration loop with condition: #while condition",
    "#global": "Starts global section (to be included in modules)",
    "#gra": "Sets trigonometric units to gradians",
    "#hide": "Hides the report contents",
    "#if": "Conditional block:\n\n    #if condition",
    "#include": "Includes external file (module):\n\n    #include filename",
    "#input": "Renders an input form to the current line and waits for user input",
    "#local": "Starts local section (not to be included in modules)",
    "#loop": "Ends an iteration block",
    "#md": "Enables/disables markdown in comments:\n\n    #md on or #md off",
    "#md off": "Disables markdown in comments",
    "#md on": "Enables markdown in comments",
    "#noc": "Shows only equations without results (no calculations)",
    "#nosub": "Does not substitute variables (no substitution)",
    "#novar": "Shows equations only with substituted values (no variables)",
    "#pause": "Calculates to the current line and waits until resumed manually",
    "#phasor": "Sets output format of complex numbers to polar phasor:\n\n    A∠φ",
    "#post": "Shows the next contents only after calculations",
    "#pre": "Shows the next contents only before calculations",
    "#rad": "Sets trigonometric units to radians",
    "#read": "Reads matrix M from a text/CSV or Excel file:\n\n    #read M from filename.txt@R1C1:R2C2 TYPE=R SEP=','",
    "#repeat": "Iteration loop: #repeat number_of_repetitions",
    "#round": "Rounds the output to n digits after the decimal point:\n\n    #round n or #round default",
    "#show": "Always shows the contents (default)",
    "#split": "Splits equations that do not fit on a single line",
    "#val": "Shows only the final result, without the equation",
    "#varsub": "Shows equations with variables and substituted values (default)",
    "#wrap": "Wraps equations that do not fit on a single line (default)",
    "#write": "Writes matrix M to a text/CSV file:\n\n    #write M to filename.xlsx@Sheet1!A1:B2 TYPE=N"
};

// Method descriptions for hover information
const methodDescriptions: { [key: string]: string } = {
	// Iterative and numerical methods and plotting:  
    "$Root": "Root finding:\n\n    $Root{f(x) @ x = a : b} or\n\n    $Root{f(x) = const @ x = a : b}",
    "$Find": "Similar to $Root, but x is not required to be a precise solution:\n\n    $Find{f(x) @ x = a : b}",
    "$Sup": "Local maximum of a function:\n\n    $Sup{f(x) @ x = a : b}",
    "$Inf": "Local minimum of a function:\n\n    $Inf{f(x) @ x = a : b}",
    "$Area": "Adaptive Gauss-Lobatto numerical integration:\n\n    $Area{f(x) @ x = a : b}",
    "$Integral": "Tanh-Sinh numerical integration:\n\n    $Integral{f(x) @ x = a : b}",
    "$Slope": "Numerical differentiation by Richardson extrapolation:\n\n    $Slope{f(x) @ x = a}",
    "$Derivative": "Numerical differentiation by complex step method:\n\n    $Derivative{f(x) @ x = a}",
    "$Sum": "Iterative sum:\n\n    $Sum{f(k) @ k = a : b}",
    "$Product": "Iterative product:\n\n    $Product{f(k) @ k = a : b}",
    "$Repeat": "Iterative expression block with counter:\n\n    $Repeat{f(k) @ k = a : b}",
    "$While": "Iterative expression block with condition:\n\n    $While{condition; expressions} ",
    "$Block": "multi-line expression block: $Block{expressions}",
    "$Inline": "Inline expression block: $Inline{expressions}",
    "$Plot": "Plot the specified function:\n\n    simple: $Plot{f1(x) & f2(x) & ... @ x = a : b},\n    parametric: $Plot{x1(t)|y1(t) & x2(t)|y2(t) & ... @ x = a : b}",
    "$Map": "2D color map of a 3D surface:\n\n    $Map{f(x; y) @ x = a : b & y = c : d}"
};

// Method descriptions for hover information
const settingDescriptions: { [key: string]: string } = {
    "PlotHeight": "Height of plot area in pixels",
    "PlotWidth": "Width of plot area in pixels",
    "PlotSVG": "Draw plots in vector (SVG) format",
    "PlotAdaptive": "Use adaptive mesh (= 1) for function plotting or uniform (= 0)",
    "PlotStep": "The size of the mesh for map plotting",
    "PlotPalette": "The number of color palette to be used for surface plots (0-9)",
    "PlotShadows": "Draw surface plots with shadows",
    "PlotSmooth": "Smooth transition of colors (= 1) or isobands (= 0) for surface plots",
    "PlotLightDir": "Direction to light source (0-7) clockwise", 
    "Precision": "Relative precision for numerical methods"
};

export function activate(context: vscode.ExtensionContext) {
    console.log('Calcpad extension is now active');

    // Register completion provider
    const completionProvider = vscode.languages.registerCompletionItemProvider(
        'calcpad',
        {
            provideCompletionItems(document: vscode.TextDocument, position: vscode.Position) {
                const items: vscode.CompletionItem[] = [];
                
                for (const item of completionItems) {
                    const completionItem = new vscode.CompletionItem(item);
                    
                    // Set completion kind based on prefix
                    if (item.startsWith('#')) {
                        completionItem.kind = vscode.CompletionItemKind.Keyword;
                        completionItem.detail = keywordDescriptions[item];
                    } else if (item.startsWith('$')) {
                        completionItem.kind = vscode.CompletionItemKind.Method;
                        completionItem.detail = methodDescriptions[item];
                    } else if (functionDescriptions[item]) {
                        completionItem.kind = vscode.CompletionItemKind.Function;
                        completionItem.detail = functionDescriptions[item];
                    } else if (settingDescriptions[item]) {
                        completionItem.kind = vscode.CompletionItemKind.Variable;
                        completionItem.detail = settingDescriptions[item];
                    } else if (/^[A-Z]/.test(item) || item.includes('°') || item.includes('Ω') || item.includes('μ')) {
                        completionItem.kind = vscode.CompletionItemKind.Unit;
                    } else {
                        completionItem.kind = vscode.CompletionItemKind.Text;
                    }
                    items.push(completionItem);
                }
                return items;
            }
        },
        '#', '$', '' // Trigger characters
    );

    // Register hover provider for function documentation
    const hoverProvider = vscode.languages.registerHoverProvider('calcpad', {
        provideHover(document: vscode.TextDocument, position: vscode.Position) {
            const range = document.getWordRangeAtPosition(position, /[#$]?[\w]+/);
            if (!range) {
                return undefined;
            }
            const word = document.getText(range);
            const description = functionDescriptions[word] ?? methodDescriptions[word] ?? keywordDescriptions[word] ?? settingDescriptions[word];
            if (description) {
                return new vscode.Hover(
                    new vscode.MarkdownString(`**${word}**\n\n${description}`)
                );
            }
            return undefined;
        }
    });

    // Register run command
    const runCommand = vscode.commands.registerCommand('calcpad.run', async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            vscode.window.showErrorMessage('No active editor');
            return;
        }

        const document = editor.document;
        if (document.languageId !== 'calcpad') {
            vscode.window.showErrorMessage('Not a Calcpad file');
            return;
        }

        // Save the file first
        await document.save();
        const filePath = document.uri.fsPath;
        const dirPath = path.dirname(filePath);
        const baseName = path.basename(filePath, '.cpd');
        const htmlPath = path.join(dirPath, baseName + '.html');
        
        // Path to Calcpad CLI - get from settings
        const config = vscode.workspace.getConfiguration('calcpad');
        const cliPath = config.get<string>('cliPath', 'C:\\Program Files\\Calcpad\\Cli.exe');
        
        // Check if CLI exists
        if (!fs.existsSync(cliPath)) {
            vscode.window.showErrorMessage(`Calcpad CLI not found at: ${cliPath}`);
            return;
        }

        // Show progress
        vscode.window.withProgress({
            location: vscode.ProgressLocation.Notification,
            title: "Running Calcpad...",
            cancellable: false
        }, async () => {
            return new Promise<void>((resolve, reject) => {
                exec(`"${cliPath}" "${filePath}" html -s`, (error, stdout, stderr) => {
                    if (error) {
                        vscode.window.showErrorMessage(`Calcpad error: ${error.message}`);
                        reject(error);
                        return;
                    }
                    
                    if (stderr) {
                        vscode.window.showWarningMessage(`Calcpad warning: ${stderr}`);
                    }

                    // Check if HTML file was created
                    setTimeout(() => {
                        if (fs.existsSync(htmlPath)) {
                            // Reuse existing panel or create new one
                            if (currentPanel) {
                                // Panel exists, just update the title and content
                                currentPanel.title = `Calcpad: ${baseName}`;
                                currentPanel.reveal(vscode.ViewColumn.Beside);
                            } else {
                                // Create new panel
                                currentPanel = vscode.window.createWebviewPanel(
                                    'calcpadOutput',
                                    `Calcpad: ${baseName}`,
                                    vscode.ViewColumn.Beside,
                                    {
                                        enableScripts: true,
                                        localResourceRoots: [vscode.Uri.file(dirPath)]
                                    }
                                );
                                
                                // Handle panel disposal
                                currentPanel.onDidDispose(() => {
                                    currentPanel = undefined;
                                });
                            }
                            
                            // Update localResourceRoots for the current file's directory
                            currentPanel.webview.options = {
                                enableScripts: true,
                                localResourceRoots: [vscode.Uri.file(dirPath)]
                            };

                            let htmlContent = fs.readFileSync(htmlPath, 'utf8');
                            const webviewUri = currentPanel.webview.asWebviewUri(vscode.Uri.file(dirPath));
                            
                            // Update relative paths in HTML to use webview URIs
                            htmlContent = htmlContent.replace(
                                /src="(?!http|data:)([^"]+)"/g,
                                `src="${webviewUri}/$1"`
                            );
                            htmlContent = htmlContent.replace(
                                /href="(?!http|#)([^"]+)"/g,
                                `href="${webviewUri}/$1"`
                            );
                            currentPanel.webview.html = htmlContent;
                            resolve();
                        } else {
                            vscode.window.showErrorMessage(`HTML output not found: ${htmlPath}`);
                            reject(new Error('HTML not generated'));
                        }
                    }, 500); // Small delay to ensure file is written
                });
            });
        });
    });

    const openCommand = vscode.commands.registerCommand("calcpad.open", async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor) {
            vscode.window.showErrorMessage('No active editor');
            return;
        }

        const document = editor.document;
        if (document.languageId !== 'calcpad') {
            vscode.window.showErrorMessage('Not a Calcpad file');
            return;
        }

        // Save the file first
        await document.save();
        const filePath = document.fileName;
        const config = vscode.workspace.getConfiguration('calcpad');
        const exePath = config.get<string>('Path', 'C:\\Program Files\\Calcpad\\Cli.exe');

        spawn(exePath, [filePath], {
            detached: true,
            stdio: "ignore"
        }).unref();
    });

    const settingsCommand = vscode.commands.registerCommand("calcpad.settings", async () => {
        const config = vscode.workspace.getConfiguration('calcpad');
        const settingsPath = config.get<string>('settingsPath', 'C:\\Program Files\\Calcpad\\Settings.Xml');
        const defaultSettings = `<?xml version="1.0" encoding="utf-8"?>
<Settings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
<Math>
    <Decimals>2</Decimals>
    <Degrees>0</Degrees>
    <IsComplex>false</IsComplex>
    <Substitute>true</Substitute>
    <FormatEquations>true</FormatEquations>
    <ZeroSmallMatrixElements>true</ZeroSmallMatrixElements>
    <MaxOutputCount>20</MaxOutputCount>
</Math>
<Plot>
    <IsAdaptive>true</IsAdaptive>
    <ScreenScaleFactor>2</ScreenScaleFactor>
    <ImagePath/>
    <ImageUri/>
    <VectorGraphics>false</VectorGraphics>
    <ColorScale>Rainbow</ColorScale>
    <SmoothScale>false</SmoothScale>
    <Shadows>true</Shadows>
    <LightDirection>NorthWest</LightDirection>
</Plot>
<Units>m</Units>
</Settings>`;

        // Create file with default content if it doesn't exist
        if (!fs.existsSync(settingsPath)) {
            try {
                fs.writeFileSync(settingsPath, defaultSettings, 'utf8');
            } catch {
                // Cannot write directly (needs admin privileges)
                const choice = await vscode.window.showWarningMessage(
                    'Settings file does not exist and requires administrator privileges to create. Create it now?',
                    'Create as Admin'
                );
                if (choice === 'Create as Admin') {
                    const escapedContent = defaultSettings.replace(/"/g, '\\"').replace(/\n/g, '`n');
                    exec(`powershell -Command "Start-Process powershell -ArgumentList '-Command', 'Set-Content -Path \\\"${settingsPath}\\\" -Value \\\"${escapedContent}\\\"' -Verb RunAs"`, (error) => {
                        if (error) {
                            vscode.window.showErrorMessage(`Failed to create settings file: ${error.message}`);
                        } else {
                            vscode.window.showInformationMessage('Settings file created. Run the command again to open it.');
                        }
                    });
                }
                return;
            }
        }

        // Check if file is writable
        try {
            fs.accessSync(settingsPath, fs.constants.W_OK);
            // File is writable, open it directly
            const document = await vscode.workspace.openTextDocument(settingsPath);
            await vscode.window.showTextDocument(document);
        } catch {
            // File is not writable (likely needs admin privileges)
            const choice = await vscode.window.showWarningMessage(
                'The settings file requires administrator privileges to edit. Would you like to open it anyway (read-only) or open VS Code as administrator?',
                'Open Read-Only',
                'Open as Admin'
            );
            if (choice === 'Open Read-Only') {
                const document = await vscode.workspace.openTextDocument(settingsPath);
                await vscode.window.showTextDocument(document);
            } else if (choice === 'Open as Admin') {
                // Launch a new VS Code instance as administrator with the file
                exec(`powershell -Command "Start-Process code -ArgumentList '${settingsPath}' -Verb RunAs"`, (error) => {
                    if (error) {
                        vscode.window.showErrorMessage(`Failed to open as administrator: ${error.message}`);
                    }
                });
            }
        }
    });

    context.subscriptions.push(
        completionProvider,
        hoverProvider,
        runCommand,
        openCommand,
        settingsCommand
    );
}
export function deactivate() {}
