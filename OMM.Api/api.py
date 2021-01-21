from flask import Flask, jsonify, request
from flask_limiter import Limiter
from flask_limiter.util import get_remote_address
from werkzeug import exceptions
import sqlite3
import numpy as np
from sklearn import neighbors

app = Flask(__name__)
limiter = Limiter(
    app,
    key_func=get_remote_address,
    default_limits=["60 per minute"]
)

DATABASE = 'S:/OMM.db'

def searialize_to_json(distance, tuple):
    return{
        'KDistance' : distance,
        'BeatmapId': tuple[1],
        'HP': tuple[2],
        'CS': tuple[3],
        'OD': tuple[4],
        'AR': tuple[5],
        'HitcircleCount': tuple[6],
        'SliderCount': tuple[7],
        'SpinnerCount': tuple[8],
        'Length': tuple[9],
        'BeatmapsetId': tuple[10],
        'Artist': tuple[11],
        'ArtistUnicode': tuple[12],
        'Title': tuple[13],
        'TitleUnicode': tuple[14],
        'Creator': tuple[15],
        'RankStatus': tuple[16],
        'CoverUrl': tuple[17],
        'Bpm' : tuple[18],
        'DifficultyName' : tuple[19],
    }

#KNN
conn = sqlite3.connect(DATABASE)
cur = conn.cursor()
cur.execute('SELECT * FROM KNN_FIT_VIEW')
all_maps = cur.fetchall()
X = np.array(all_maps)
classes = [1] * len(all_maps)
conn.close()
y = np.array(classes)

clf = neighbors.KNeighborsClassifier(n_neighbors=1)
clf.fit(X, y);

#Api endpoints
@app.route('/', methods=['GET'])
@app.route('/info', methods=['GET'])
def get_info():
    return jsonify({'info' : 'No info'})

@app.route('/api/knn/maps', methods=['GET'])
@limiter.limit("30/minute")
def get_all_map_ids():
    with sqlite3.connect(DATABASE) as connection:
        cursor = connection.cursor()
        cursor.execute("SELECT BeatmapId FROM Beatmap")
        ids = cursor.fetchall()

    return jsonify([e[0] for e in ids])


@app.route('/api/knn/ranked', methods=['GET'])
@limiter.limit("30/minute")
def get_similiar_maps():
    beatmap_id = request.args.get('id', default=None, type=int)
    match_count = request.args.get('count', default=10, type=int)

    if (beatmap_id < 1):
        return "BeatmapId is not valid", 400

    if (match_count < 1):
        return "Count cant be smaller than 1", 400

    if (match_count > 101):
        return "You cannot request more than 100 maps", 400

    with sqlite3.connect(DATABASE) as connection:
        cursor = connection.cursor()
        cursor.execute("""
        SELECT 
            HP,
            CS,
            OD,
            AR,
            BeatDivisor,
            TotalHitCircles,
            TotalSliders,
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
            Bpm,BpmMax,
            Total90DegreeJumps,
            Total180DegreeJumps 
        FROM 
            Beatmap 
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
    app.run('localhost', 55555)