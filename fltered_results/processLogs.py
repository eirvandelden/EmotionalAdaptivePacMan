#!/usr/bin/env python
# encoding: utf-8
"""
processLogs.py

Created by Etienne van Delden on 2011-01-31.
Copyright (c) 2011 __MyCompanyName__. All rights reserved.
"""

import sys
import os

##########################################
##                                      ##
##  Define GLOBALS                      ##
##                                      ##
##########################################
folder = "01A/processed/"

outputFile = "output.txt"
inputCalib = folder+"Calib.dat"
inputAdapt = folder+"Adaptive.dat"

inputPressure1 = folder+"1PacmanEvents.log"
inputPressure2 = folder+"2PacmanEvents.log"

inputMCalib = folder+"calibMobi_filt.txt"
inputMAdapt0 = folder+"adaptMobi0_filt.txt"
inputMAdapt1 = folder+"adaptMobi1_filt.txt"
inputMAdapt2 = folder+"adaptMobi2_filt.txt"

starttime = 0



##########################################
##                                      ##
##  Define CLASSES                      ##
##                                      ##
##########################################

class Point:
    category = 4
    emg0 = -1
    gsr = -1
    pressure = -1
    speed = -1
    time = -1
    start = -1
    end = -1


##########################################
##                                      ##
##  Define FUNCTIONS                    ##
##                                      ##
##########################################


##########################################
##  Process .DAT files                  ##
##########################################
def processData(filename):
    """This functions reads all lines in a calib.data file, and creates an 
    point object for each line"""
    global starttime
    
    with open(filename, 'r') as input: 
        count = 0
        result = [ [],[],[] ]
        first = True


        #For each line, strip and split it, then append it to lines.
        for line in input: 
            tokens = line.strip().split() 
            temp = Point()            
            
            #skip non essential lines, use count to take adapt.dat into account
            if tokens[0].find('/') > -1:
                print "skipped comment line"
            elif (tokens[0].find('<') != -1) and first:
                starttime = float(tokens[2])
                
                first = False
                print "skipped comment line"
            elif (tokens[0].find('<') != -1):
                count = count + 1
            else:   #Put essential data in point, add to list
                temp.category = int(tokens[0])
                temp.speed = float(tokens[5])
                temp.time = float(tokens[7])/1000
                temp.start = float(tokens[6])/1000
                temp.end = float(tokens[7])/1000
                result[count].append(temp) 
            # Clean up temporary point
            del temp
    return result


##########################################
##  READ Pressure files                 ##
##########################################    
def processPressure(filename):
    """This functions reads all lines in a xPressure.log file, and creates an 
    point object for each line"""

    with open(filename, 'r') as input: 
        count = 0
        result = []
        first = True


        #For each line, strip and split it, then append it to lines.
        for line in input: 
            tokens = line.strip().split() 
            temp = Point()            
            
            #skip non essential lines, use count to take adapt.dat into account
            if first:
                first = False
                #print "skipped comment line", tokens[0]
            else:   #Put essential data in point, add to list
                temp.speed = int(tokens[6])
                temp.time = float(tokens[0])
                temp.pressure = int(max(tokens[2:6]))
                result.append(temp)     
            # Clean up temporary point
            del temp

    return result
    
##########################################
##  Read Mobi logs                      ##
##########################################
def processMobi(filename):
    """This functions reads all lines in a xPressure.log file, and creates an 
    string arry for each line"""

    with open(filename, 'r') as input: 
        count = 0
        result = []
        skipFirst = False

        #For each line, strip and split it, then append it to lines.
        for line in input: 
            tokens = line.strip().split() 
            temp = Point()            
            
            #skip non essential lines, use count to take adapt.dat into account
            if skipFirst:
                skipFirst = False
                #print "skipped comment line", tokens[0]
            else:   #Put essential data in point, add to list
                #print tokens
                temp.time = float(tokens[0])
                temp.emg0 = float(tokens[3])
                temp.gsr = float(tokens[4])
                #temp.speed = -1
                result.append(temp) 
                
            # Clean up temporary point
            del temp

    return result

##########################################
##  ENTWINE PRESSURE & Datalog          ##
##########################################
def entwinePressure(datalog, pressureLog):
    """ This function returns a new log with the pressureLog entwined
    with the data log """
    
    indexCounter = 0
    result = []
    category = datalog[0].category
    first = True
    
    for point in pressureLog:
        for i in range(indexCounter, len(datalog)):
            if ((datalog[i].start) < point.time) and ((datalog[i].end)  > point.time):
                #print "BETWEEN"
                if first:
                    #result.append(datalog[i])
                    first = False
                
                if indexCounter < i:
                    #result.append(datalog[i])
                    indexCounter = i
                    category = datalog[i].category
                
                #Adjust category of point and add
                point.category = category
                result.append(point)
                break
            #else:
            #    print "SKIPPED"

    print len(result)           
    return result

##########################################
##  ENTWINE MOBI & mobielog             ##
##########################################    
def entwineMobi(datalog, mobiLog):
    """ This function returns a new log with the pressureLog entwined
    with the data log """
    
    global starttime
    print str(starttime)
    
    indexCounter = 0
    result = []
    category = datalog[0].category
    first = True
    

    
    checkTime = starttime+10
    mustUpdateCheckTime = True
    
    
    
    for point in mobiLog:
        for i in range(indexCounter, len(datalog)):
            #print "chectime: " + str(checkTime)
            #print "point time: " + str(point.time)
            #print ""
        
            if ((checkTime - 6) < point.time) and ( point.time < checkTime ):
                #print "BETWEEN"
                if first:
                    #result.append(datalog[i])
                    first = False
                
                if indexCounter < i:
                    #result.append(datalog[i])
                    indexCounter = i
                    category = datalog[i].category
                if not(mustUpdateCheckTime):
                    mustUpdateCheckTime = True
                
                #Adjust category of point and add
                point.category = category
                point.speed = datalog[i].speed
                result.append(point)
                break
            else:
                if mustUpdateCheckTime and (checkTime > point.time):
                    checkTime = checkTime +10
                    mustUpdateCheckTime = False
            #    print "SKIPPED"
    print len(result)           
    return result

def processFiles(): 
    """This functions reads all lines in a file, and creates an string arry for each 
        line"""
    print "Reading Logs"
    # Read the calibration data
    calibLog = processData(inputCalib)
        #post: 

    # Read the Adaptive loop and create three lists with points
    #adaptLog = processData(inputAdapt)

    # Read the pressure log
    print "Reading Pressure logs"
    pressureLog1 = processPressure(inputPressure1)
    pressureLog2 = processPressure(inputPressure2)
    
    print "Reading Mobi Log"
    # Read the mobi log
    # Read the Mobi Calibration logs
            #calibMobi_filt
    mobiLog = processMobi(inputMCalib)
    
    # Read the three Mobi Adaptive logs
    
    
    print "Entwining Pressure log"
    #Entwine Pressure log with Calibration data
    parsedPressureLog = entwinePressure(calibLog[0], pressureLog1)
        #post: parsedpressureLog is een lijst van points, met tijd en pressure
    print "Entwining Mobi Log"
    #Entwine Mobi log with Calibration data
    parsedMobiLog = entwineMobi(calibLog[0], mobiLog)
    print "Writing output"
    writeOutput(parsedMobiLog, folder+"calibMobi.txt", "mobi")

        
   # for i in range(3):
        #Entwine the right adaptive loop and pressure log
       # print "Entwining PressureLog: Adaptive loop ", i
    #    templist = entwinePressure(adaptLog[i], pressureLog2)
    #    writeOutput(templist, "processed/adaptPressure" + str(i) + ".txt", "pressure")
    #    del templist
        #Entwine the right adaptive loop and mobi log
   #     print "Entwining MobiLog: Adaptive loop ", i
   #     templist = entwineMobi(adaptLog[i], mobiLog)
   #     writeOutput(templist, folder+"adaptMobi" + str(i) + ".txt", "mobi")
   #     del templist
        
def writeOutput(lists, outputFileName, mode):
    with open(outputFileName, 'w') as output:
        print >>output, "// Time, EMG_Zygo, GSR,  Ghost speed"
#        print len(lists)
        for i in lists:
#            print i.time
            if i.start > -1:
                calibpoint = 1
            else:
                calibpoint = 0
            
            if mode == "pressure":
                print >>output, i.time, i.category, i.pressure, calibpoint
            else:
                print >>output, i.time, i.category, i.emg0, i.gsr,  i.speed


def main():
    """ The main loop, it calls all the others """
    # Read in files, according to a certain order
    
    processFiles()

    #output test data to console
    #writeOutput(logs, "output.txt")

##########################################
##                                      ##
##  Run Main Loop                       ##
##                                      ##
##########################################
if __name__=="__main__": 
   # if len(sys.argv) != 2: 
   #     print __doc__ 
   # else: 
   #     start(sys.argv[1])
    main()
