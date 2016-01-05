
DO IF  (ExpDiffKalib = 0).
RECODE EMGZyg GSR (ELSE=Copy) INTO EMGZygmakkelijk GSRmakkelijk.
END IF.
EXECUTE.

DO IF  (ExpDiffKalib = 1).
RECODE EMGZyg GSR (ELSE=Copy) INTO EMGZyggoed GSRgoed.
END IF.
EXECUTE.

DO IF  (ExpDiffKalib = 2).
RECODE EMGZyg GSR (ELSE=Copy) INTO EMGZygmoeilijk GSRmoeilijk.
END IF.
EXECUTE.

DESCRIPTIVES VARIABLES=EMGZygmakkelijk GSRmakkelijk EMGZyggoed GSRgoed EMGZygmoeilijk GSRmoeilijk
  /STATISTICS=MEAN STDDEV MIN MAX.
