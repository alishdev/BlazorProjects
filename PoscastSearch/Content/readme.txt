This is content building stage
Plan:
1. Download 100 episodes from YBCK - done downloaded 500 episodes with taddy_client.py
2. Transcribe all episodes - with Transcribe.py
3. Use AI to split (or not – use text) transcriptions and summarize each file. 
4. Then use AI to read summaries and build a book “How to apply to college”. 
4a. First create table of contents, and then parse each file and build a book with citations (as links to episodes).

5. Build a Chroma database to store transcriptions - put filename and position/chunk as metadata.
for i in tqdm(range(0, len(train), 1000)):
    documents = [description(item) for item in train[i: i+1000]]
    vectors = model.encode(documents).astype(float).tolist()
    metadatas = [{"category": item.category, "price": item.price} for item in train[i: i+1000]]
    ids = [f"doc_{j}" for j in range(i, i+1000)]
    collection.add(
        ids=ids,
        documents=documents,
        embeddings=vectors,
        metadatas=metadatas
    )
def find_similars(item):
    results = collection.query(query_embeddings=vector(item).astype(float).tolist(), n_results=5)
    documents = results['documents'][0][:]
    prices = [m['price'] for m in results['metadatas'][0][:]]
    return documents, prices


Downloading issues
There is no a good API to download podcast episodes.
One possible solution is to use the Spotify API, but I am not sure if it allows to download episodes.
Another place is to use Youtube Music, podcasters were supposed to move their content here, but many did not. 
Still it is good API to work on as a possible solution if Spotify does not work.
Also check other podcast APIs, but they are not free.
If I can find a solution, then maybe building one can be a good project.
Also check RSS feed and find out what to do if feed is not complete.
