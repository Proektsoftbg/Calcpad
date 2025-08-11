import openseespy.opensees as ops
# Мерни единици
m = 1; N = 1; kN = 1000*N; Pa = N/m**2; MPa = Pa*1e6; GPa = Pa*1e9; cm = m/100
# Характеристики на модела
a = 3*m; b = 6*m # Разстояние между възлите
E0 = 206*GPa # Начален модул на еластичност
fy = 500*MPa # Граница на провлачване
fu = 600*MPa #  Якост на опън
εy = fy/E0 # Деформация при провлачване
εu = 0.02 # Деформация при скъсване
E1 = (fu - fy)/(εu - εy) # Остатъчен модул на еластичност
d = 2*cm # Диаметър на прътите
A = 3.14159265359*d**2/4 # Площ на напречното сечение
F = 70*kN # Вертикално натоварване в средния възел
P = 20*kN # Сила на предварително напрягане
σp = P/A # Преврарително напрежение
# Създаване на модела
ops.model('basic','-ndm',2,'-ndf',2)
ops.node(1,0,0); ops.fix(1,1,1)
ops.node(2,a,-1e-6*cm) # Малка корекция на положението спрямо правата линия
ops.node(3,a+b,0); ops.fix(3,1,1)
ops.uniaxialMaterial('ElasticBilin',1,E0,E1,εy)
ops.uniaxialMaterial('InitStressMaterial',2,1,σp)
ops.element('corotTruss',1,1,2,A,2)
ops.element('corotTruss',2,2,3,A,2)
ops.timeSeries('Constant',1)
ops.pattern('Plain',1,1)
ops.load(2,0,-F)
# Стартиране на анализа
ops.analysis('Static','-noWarnings')
ops.analyze(1)
ops.reactions()
# Отпечатване на резултатите
print(f"Премествания: {["%.2fmm" % (f*1000) \
for f in ops.nodeDisp(2)]}")
print(f"Опорни реакции: {["%.2fkN" % (f/1000) \
for f in ops.nodeReaction(1)]}, {["%.2fkN" % (f/1000) \
for f in ops.nodeReaction(3)]}")
print(f"Осови сили в прътите: {["%.2fkN" % (f/1000) \
for f in ops.basicForce(1)]}, {["%.2fkN" % (f/1000) \
for f in ops.basicForce(2)]}")
