
DATASET ACTIVATE DataSet1.

COMPUTE P=(P+1000)/2.
EXECUTE.

DO IF  (Category=0).
RECODE P (ELSE=Copy) INTO Presmakkelijk.
END IF.
VARIABLE LABELS  Presmakkelijk 'pressure bij makkelijk'.
EXECUTE.

DO IF  (Category=1).
RECODE P (ELSE=Copy) INTO Presgoed.
END IF.
VARIABLE LABELS  Presgoed 'pressure bij goed'.
EXECUTE.

DO IF  (Category=2).
RECODE P (ELSE=Copy) INTO Presmoeilijk.
END IF.
VARIABLE LABELS  Presmoeilijk 'pressure bij moeilijk'.
EXECUTE.

DESCRIPTIVES VARIABLES=Presmakkelijk Presgoed Presmoeilijk
  /STATISTICS=MEAN STDDEV MIN MAX.
