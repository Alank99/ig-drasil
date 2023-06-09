"use strict"

import crypto from 'crypto'

const prefix = "/api/user/";

export function addEndpoints(app, conn) {
    const connection = conn;

    async function hashPassword(plainPassword) {
        const hash = crypto.createHash('sha256');
        hash.update(plainPassword);
        return hash.digest('hex');
    }

    // get list loot tables
    app.get(prefix + "", async (request, response)=>{
        try
        {
            const query = `select * from user`
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
    app.post(prefix + "login", async (request, response)=>{
        try
        {
            console.log(await hashPassword(request.body.password))
            const query = `select user_id from user where email = '${request.body.email}' and password = '${await hashPassword(request.body.password)}';`
            const [results1, fields1] = await connection.execute(query);
    
            response.json(results1)
        }
        catch(error)
        {
            response.status(500)
            response.json(error)
            console.log(error)
        }
    });

    // make a new user
    app.post(prefix + "new", async (request, response)=>{
        try
        {
            let query = `insert into user (name, email, password) values ('${request.body.name}', '${request.body.email}', '${await hashPassword(request.body.password)}');`
            const [results, fields] = await connection.execute(query);

            query = `select user_id from user where email = '${request.body.email}';`
            const [results1, fields1] = await connection.execute(query);
    
            console.log(`${results1.length} rows returned, created users successfully!!`)
            response.json(results1)
        }
        catch(error)
        {
            response.status(500)
            response.json(error)
            console.log(error)
        }
    });
}
