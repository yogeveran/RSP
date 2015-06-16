__author__ = 'eranyogev'
import csv
import json

csvfile = open('ratings.csv', 'r')
jsonfile = open('ratings.json', 'w')

fieldnames = ("userId","movieId","rating","timestamp")
reader = csv.DictReader(csvfile, fieldnames)
c = True
for row in reader:
    if c:
        c = False
        continue
    json.dump(str(row), jsonfile)
    jsonfile.write(str('\n'))
