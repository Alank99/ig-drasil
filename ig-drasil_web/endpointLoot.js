const prefix = "/api/buffs/";

export function addEndpoints(app, conn) {

    const connection = conn;

    // get list loot tables
    app.get(prefix + "", async (request, response)=>{
        try
        {
            const query = `select * from loot_table`
            const [results, fields] = await connection.execute(query);
    
            console.log(`${results.length} rows returned`)
            response.json(results)
        }
        catch(error)
        {
            response.status(500)
            response.json(error)
            console.log(error)
        }
    });

    // get list of drops for given loot table
    app.get(prefix + ":id", async (request, response)=>{
        try
        {
            // TODO: cambiar a que haga los joins del loot
            const query = `select * from loot where id = ${request.params.id}`
            const [results, fields] = await connection.execute(query);
    
            console.log(`${results.length} rows returned`)
            response.json(results)
        }
        catch(error)
        {
            response.status(500)
            response.json(error)
            console.log(error)
        }
    });


    // get loot info
    app.get(prefix + "info/:id", async (request, response)=>{
        try
        {
            const query = `select * from loot where id = ${request.params.id}`
            const [results, fields] = await connection.execute(query);
    
            console.log(`${results.length} rows returned`)
            response.json(results)
        }
        catch(error)
        {
            response.status(500)
            response.json(error)
            console.log(error)
        }
    });
}
