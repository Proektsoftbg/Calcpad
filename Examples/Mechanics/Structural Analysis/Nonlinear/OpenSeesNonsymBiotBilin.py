import openseespy.opensees as ops
# Units of measurement
m = 1; N = 1; kN = 1000*N; Pa = N/m**2; MPa = Pa*1e6; GPa = Pa*1e9; cm = m/100
# Model properties
a = 3*m; b = 6*m # Joint spacing
E0 = 206*GPa # Initial modulus of elasticity
fy = 500*MPa # Yield stress
fu = 600*MPa # Ultimate tensile stress
εy = fy/E0 # Yield strain
εu = 0.02 # Ultimate strain
E1 = (fu - fy)/(εu - εy) # Residual modulus of elasticity
d = 2*cm # Cross section diameter
A = 3.14159265359*d**2/4 # Cross section area
F = 70*kN # Vertical load at the intermediate joint
P = 20*kN # Prestressing force
σp = P/A # Initial stress
# Model creation
ops.model('basic','-ndm',2,'-ndf',2)
ops.node(1,0,0); ops.fix(1,1,1)
ops.node(2,a,-1e-6*cm) # Small
ops.node(3,a+b,0); ops.fix(3,1,1)
ops.uniaxialMaterial('ElasticBilin',1,E0,E1,εy)
ops.uniaxialMaterial('InitStressMaterial',2,1,σp)
ops.element('corotTruss',1,1,2,A,2)
ops.element('corotTruss',2,2,3,A,2)
ops.timeSeries('Constant',1)
ops.pattern('Plain',1,1)
ops.load(2,0,-F)
# Running the analysis
ops.analysis('Static','-noWarnings')
ops.analyze(1)
ops.reactions()
# Printing the results
print(f"Displacements: {["%.2fmm" % (f*1000) for f in ops.nodeDisp(2)]}")
print(f"Support reactions: {["%.2fkN" % (f/1000) \
for f in ops.nodeReaction(1)]}, {["%.2fkN" % (f/1000) \
for f in ops.nodeReaction(3)]}")
print(f"Element axial forces: {["%.2fkN" % (f/1000) \
for f in ops.basicForce(1)]}, {["%.2fkN" % (f/1000) 
for f in ops.basicForce(2)]}")