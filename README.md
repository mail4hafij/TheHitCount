# TheHitCount
This service should make a search against two or more search engines (e.g., Google, Bing, Yahoo, Twitter, Web Search, Algolia, AltaVista…) and present the total number of search hits from each search engine. If the user enters several words, this service should make a search for each word, summarize the number of hits, and then present the sum from each search engine.

## Projects
There are 5 projects in this solution

1. **Web**: A simple website with an input field so that users can make a search.
2. **Rest**: A simple rest api that accepts post request to make a search.
3. **Core**: Implements the HitCountService. All the logics are written here.
4. **Functions**: Implements a Durable Funciton to orchestrate search for different search engines.
5. **Common**: Common dependency for the other project.

The key ideas behind this projects are -
* Seperation of concerns.
* Use concurrently running tasks to make search quicker (way 1).
* Use orchestration mechanism to make search quicker (way 2).

## Conceptual model (way 1)
<img src="hitcount.png" />

## Conceptual model (way 2)




