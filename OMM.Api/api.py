import uvicorn
import aiosqlite
import sqlite3
import pathlib
from fastapi import FastAPI
from fastapi.responses import PlainTextResponse
import numpy as np
from sklearn import neighbors

app = FastAPI()

all_map_ids = None

DATABASE = 'OMM.db'

#KNN
with sqlite3.connect(DATABASE) as conn:
    cur = conn.cursor()
    cur.execute('SELECT * FROM KNN_FIT_VIEW')
    all_maps = cur.fetchall()
    X = np.array(all_maps)
    classes = [1] * len(all_maps)
    cur.close()

    cur = conn.cursor()
    cur.execute("select beatmapid from beatmaps")
    all_map_ids = str(([e[0] for e in cur.fetchall()])).replace(" ", "")
    cur.close()

y = np.array(classes)

clf = neighbors.KNeighborsClassifier(n_neighbors=1)
clf.fit(X, y)

#free ram
all_maps = None
X = None
y = None
classes = None
conn = None
cur = None

#Api endpoints
@app.get('/')
@app.get('/info')
def get_info():
    return {'info' : 'No info'}

@app.get('/version')
def get_info():
    return "0.4"

@app.get('/api/knn/maps')
def get_all_map_ids():
    return PlainTextResponse(all_map_ids, status_code=200)

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
        'MapLink' : 'https://osu.ppy.sh/b/' + str(tuple[4]),
        'OsuDirectLink' : 'osu://b/' + str(tuple[4]),
    }

@app.get('/api/knn/search')
async def get_similiar_maps(id: int, count: int=10):
    if (id < 1):
        return PlainTextResponse("BeatmapId is not valid", status_code=400)

    if (count < 1):
        return PlainTextResponse("Count cant be smaller than 1", status_code=400)

    if (count > 50):
        return PlainTextResponse("Requested to many maps", status_code=400)

    async with aiosqlite.connect(DATABASE) as connection:
        cursor = await connection.execute("""
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
            Total180DegreeJumps,
            TotalSliderLength,
            TotalSliderDegrees,
            TotalSliderAngles,
            AvgFastestSliderSpeed
        FROM 
            Beatmaps
        WHERE 
            BeatmapId = ?""", (id,))
        selected_map = await cursor.fetchone()
        await cursor.close()

    if (selected_map is None):
        return PlainTextResponse("Beatmap not found", status_code=404)

    count = count + 1
    kneighbors = clf.kneighbors([selected_map], count)
    neighbor_ids = kneighbors[1][0] + 1
    distances = kneighbors[0][0]
    distances = distances[1:]
    
    query = np.array2string(neighbor_ids, separator=' OR Id=')
    query = 'SELECT * FROM BEATMAP_MATCH_VIEW WHERE Id=' + query.strip('[]') + ' ORDER BY CASE Id'
    for i, val in enumerate(neighbor_ids):
        query = query + ' WHEN ' + str(val) + ' THEN ' + str(i)
    query = query + " END"

    async with aiosqlite.connect(DATABASE) as connection:
        cursor = await connection.execute(query)
        map_datas = await cursor.fetchall()
        map_datas = map_datas[1:]
        await cursor.close()

    return ([searialize_to_json(distances[i], e) for i, e in enumerate(map_datas)])

if __name__ == '__main__':
    uvicorn.run("api:app", port=55555)