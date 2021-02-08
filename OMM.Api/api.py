import uvicorn
import aiosqlite
import pathlib
from fastapi import FastAPI
from fastapi.responses import PlainTextResponse
import sqlite3
import numpy as np
from sklearn import neighbors

app = FastAPI()
h = hpy()

DATABASE = str(pathlib.Path(__file__).parent.absolute()) + '\OMM.db'

#KNN
with sqlite3.connect(DATABASE) as conn:
    cur = conn.cursor()
    cur.execute('SELECT * FROM KNN_FIT_VIEW')
    all_maps = cur.fetchall()
    X = np.array(all_maps)
    classes = [1] * len(all_maps)
    cur.close()

y = np.array(classes)

clf = neighbors.KNeighborsClassifier(n_neighbors=1)
clf.fit(X, y)

#Api endpoints
@app.get('/')
@app.get('/info')
async def get_info():
    return {'info' : 'No info'}

#@app.get('/api/knn/maps')
#def get_all_map_ids():
#    with sqlite3.connect(DATABASE) as connection:
#        cursor = connection.cursor()
#        cursor.execute("SELECT BeatmapId FROM Beatmaps WHERE BeatmapId = 1")
#        ids = cursor.fetchall();
#        cursor.close()
        
#    return ([e[0] for e in ids])

@app.get('/api/knn/maps')
async def get_all_map_ids():
    async with aiosqlite.connect(DATABASE) as connection:
        cursor = await connection.execute("SELECT BeatmapId FROM Beatmaps")
        items = await cursor.fetchall()
        await cursor.close()
        print(h.heap())
        return ([e[0] for e in items])

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

@app.get('/api/knn/ranked')
async def get_similiar_maps(id: int, count: int=10):
    if (id < 1):
        return PlainTextResponse("BeatmapId is not valid", status_code=400)

    if (count < 1):
        return PlainTextResponse("Count cant be smaller than 1", status_code=400)

    if (count > 51):
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
            Total180DegreeJumps 
        FROM 
            Beatmaps
        WHERE 
            BeatmapId = ?""", (id,))
        selected_map = await cursor.fetchone()
        await cursor.close();

    if (selected_map is None):
        return PlainTextResponse("Beatmap not found", status_code=404)

    kneighbors = clf.kneighbors([selected_map], count)
    neighbor_ids = kneighbors[1][0] + 1
    distances = kneighbors[0][0]
    
    query = np.array2string(neighbor_ids, separator=' OR Id=')
    query = 'SELECT * FROM BEATMAP_MATCH_VIEW WHERE Id=' + query.strip('[]') + ' ORDER BY CASE Id'
    for i, val in enumerate(neighbor_ids):
        query = query + ' WHEN ' + str(val) + ' THEN ' + str(i)
    query = query + " END"

    async with aiosqlite.connect(DATABASE) as connection:
        cursor = await connection.execute(query)
        map_datas = await cursor.fetchall()
        await cursor.close();
    
    print(h.heap())
    return ([searialize_to_json(distances[i], e) for i, e in enumerate(map_datas)])

if __name__ == '__main__':
    uvicorn.run("api:app", port=55555)