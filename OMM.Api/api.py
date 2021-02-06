from flask import Flask, jsonify, request
from werkzeug import exceptions
import logging
import sqlite3
import numpy as np
from sklearn import neighbors

app = Flask(__name__)

log = logging.getLogger('werkzeug')
log.setLevel(logging.ERROR)

DATABASE = 'S:/OMM.db'

#KNN
with sqlite3.connect(DATABASE) as conn:
    cur = conn.cursor()
    cur.execute('SELECT * FROM KNN_FIT_VIEW')
    all_maps = cur.fetchall()
    X = np.array(all_maps)
    classes = [1] * len(all_maps)

y = np.array(classes)

clf = neighbors.KNeighborsClassifier(n_neighbors=1)
clf.fit(X, y);

#Api endpoints
@app.route('/')
@app.route('/info', methods=['GET'])
def get_info():
    return jsonify({'info' : 'No info'})

@app.route('/api/knn/maps', methods=['GET'])
def get_all_map_ids():
    with sqlite3.connect(DATABASE) as connection:
        cursor = connection.cursor()
        cursor.execute("SELECT BeatmapId FROM Beatmaps")
        ids = cursor.fetchall()

    return jsonify([e[0] for e in ids])


def searialize_to_json(distance, tuple):
    return{
        'AR': tuple[1],
        'Artist': tuple[2],
        'ArtistUnicode': tuple[3],
        'BeatmapId': tuple[4],
        'BeatmapSetId': tuple[5],
        'Bpm': tuple[6],
        'BpmMax': tuple[7],
        'CS': tuple[8],
        'DifficultyName': tuple[9],
        'HP': tuple[10],
        'KDistance' : distance,
        'Length': tuple[11] / 10,
        'Mapper': tuple[12],
        'OD': tuple[13],
        'Title': tuple[14],
        'TitleUnicode': tuple[15],
        'TotalHitCircles': tuple[16],
        'TotalSliders': tuple[17],
        'TotalSpinners' : tuple[18],
    }

@app.route('/api/knn/ranked', methods=['GET'])
def get_similiar_maps():
    beatmap_id = request.args.get('id', default=None, type=int)
    match_count = request.args.get('count', default=10, type=int)

    if (beatmap_id < 1):
        return "BeatmapId is not valid", 400

    if (match_count < 1):
        return "Count cant be smaller than 1", 400

    if (match_count > 51):
        return "You cannot request more than 50 maps", 400

    with sqlite3.connect(DATABASE) as connection:
        cursor = connection.cursor()
        cursor.execute("""
        SELECT 
            HP,
            CS,
            OD,
            AR,
            BeatDivisor,
            CalcTotalHitCircles,
            CalcTotalSliders,
            TotalSpinners,
            Length,
            TotalPixelsTraveled,
            TotalDoubles,
            TotalDoublesAfterSliderEnd,
            TotalTriplets,
            TotalQuadruplets,
            TotalBursts,
            TotalStreams,
            TotalDeathStreams,
            LongestStream,
            TotalJumpStreams,
            TotalSpacedStreamPixels,
            CalcBpm,
            CalcBpmMax,
            Total90DegreeJumps,
            Total180DegreeJumps 
        FROM 
            Beatmaps
        WHERE 
            BeatmapId = ?""", (beatmap_id, ))
        selected_map = cursor.fetchone()

    if (selected_map is None):
        return "Beatmap not found", 404

    kneighbors = clf.kneighbors([selected_map], match_count)
    neighbor_ids = kneighbors[1][0] + 1
    distances = kneighbors[0][0]
    
    query = np.array2string(neighbor_ids, separator=' OR Id=')
    query = 'SELECT * FROM BEATMAP_MATCH_VIEW WHERE Id=' + query.strip('[]') + ' ORDER BY CASE Id'
    for i, val in enumerate(neighbor_ids):
        query = query + ' WHEN ' + str(val) + ' THEN ' + str(i)
    query = query + " END"

    with sqlite3.connect(DATABASE) as connection:
        cursor = connection.cursor()
        cursor.execute(query)
        map_datas = cursor.fetchall()
    
    return jsonify([searialize_to_json(distances[i], e) for i, e in enumerate(map_datas)])

if __name__ == '__main__':
    app.run('0.0.0.0', 55555)