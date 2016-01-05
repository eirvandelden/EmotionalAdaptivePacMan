
DATASET ACTIVATE DataSet2.
GLM Boredom1 Boredom2 Boredom3
  /WSFACTOR=diffuculty 3 Polynomial 
  /MEASURE=boredom 
  /METHOD=SSTYPE(3)
  /EMMEANS=TABLES(diffuculty) COMPARE ADJ(BONFERRONI)
  /CRITERIA=ALPHA(.05)
  /WSDESIGN=diffuculty.

GLM Frustration1 Frustration2 Frustration3
  /WSFACTOR=diffuculty 3 Polynomial 
  /MEASURE=frustration 
  /METHOD=SSTYPE(3)
  /EMMEANS=TABLES(diffuculty) COMPARE ADJ(BONFERRONI)
  /CRITERIA=ALPHA(.05)
  /WSDESIGN=diffuculty.

GLM PosAff1 PosAff2 PosAff3
  /WSFACTOR=diffuculty 3 Polynomial 
  /MEASURE=posaff 
  /METHOD=SSTYPE(3)
  /EMMEANS=TABLES(diffuculty) COMPARE ADJ(BONFERRONI)
  /CRITERIA=ALPHA(.05)
  /WSDESIGN=diffuculty.
