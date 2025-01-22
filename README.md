# Bloggo
Minimum Viable Product

---

Mål:
1. Oversikt over blogger og forfattere.
2. En forfatter skal kunne opprette flere innlegg.
3. Innlegg skal kunne drøftes før publisering.
4. En forfatter skal kunne sammarbeide med andre forfattere.
5. Det skal kunne datostemple innleggets opprettelse og endringer.
6. Det skal kunne "tracke" anntall visninger til spesifikke innlegg.
7. Data skal lagres i SQLite, med abstraksjoner og relasjoner.
8. Det skal være en front-end, brukergrensesnittet skal være forskjellig
   basert på om brukeren aktivt arbeider på ett spesifikt innlegg, eller 
   om de er en leser av ett innlegg.

## Krav for datahåndtering
1. Sensitive og priviligert informasjon må kunne kommuniseres kryptert, 
   dette gjelder spesielt for innlogging fra httpForms. Innloggingsprosessen 
   skal returnere en token, for å opprette og vedlikeholde innloggingsøkten.
    - Brukere skal kunne opprette nye sidebrukere fra brukernavn og passord.
    - Passord skal ikke lagres i plaintext, og skal ikke eksistere i
      minnet lengre enn nødvendig.
        - Eventuelle valg av backend språk kan bety at minnet må manuelt
          forsøpples, f.eks. pga. forsinket GC reclaimation.
    - Utenom forfattere av innlegg, skal brukernavn anonymiseres.
2. Det skal være en oversikt over innlegg.
    - Tittell på innlegg.
    - Forfattere.
    - Opprettelses dato.
    - Sist endret dato.
    - Anntall visninger.
3. Det skal være en oversikt over forfattere og deres relasjoner til innlegg.
    - Eier av innlegg.
    - Involvering i innlegg (med-forfatterskap).
4. Brukere skal kunne utføre CRUD operasjoner.
    - En registrert bruker skal kunne opprette ett innlegg. 
        - CREATE
    - En bruker skal kunne lese innlegg.
        - READ
    - En eier av ett innlegg skal kunne legge til og fjerne medforfatter(e). 
        - UPDATE
    - En forfatter eller medforfatter skal kunne endre innhold i innlegg.
        - UPDATE
    - En eier av ett innlegg skal kunne slette innlegget.
        - DELETE
        - En sletting vil sette innlegget i en kø som utfører destruering ved
          senere tid. Dette tillater at en angrer seg og kan gjennopprette innlegget.
    
## Utførelse av krav
Valg av backend-språk avgjøres ved senere dato. Designpattern er fastsatt, 
MVC (Model, View, Controller). SQLite databasen anvendes som "source of truth" for dataen. 
Frontend utformes i HTML, CSS, og Javascript.

### Backend
- Det skal lages modeller for data
    1. Brukere.
    2. Innlegg.
    3. EierRelasjoner.
    4. ForfatterRelasjoner.
    5. BrukerDTO.
    6. InnleggDTO.
- DTO (Data Transfer Objects) anvendes som en abstraksjon over data i databasen 
  og for å holde "business logic" separat fra objekter som anvendes av API-en.
- Controllers er ansvarlig for å hente ut og mate inn data. 
  Det skal være både en bruker controller og en innlegg controller.
- Frontend skal serveres fra statiske filer.
    - Default route og fallback route.
- Det skal også være en separat route til en login side, også fra statisk fil.

### Frontend
- HTML, CSS, og Javascript anvendes for å generere brukergrensesnittet.
- View genereres fra data i databasen.
    - Data motatt fra backend skal modelleres, og views basers på disse.
    - Det skal ikke antas en spesifikk eksistens, resultater skal bli `.map()`-et
      over, og `Object.keys` skal anvendes fremfor indexering.
- Det skal være en frontpage / hero page.
    - 

## Sekvensdiagrammer

1. Default route til API, /GET
```mermaid
sequenceDiagram
    HttpRequest ->>+ ApiDefaultRoute: /GET
    ApiDefaultRoute ->>+ StaticFolder: find index.html
    StaticFolder -->>- HttpRequest: serve index.html
```

2. Sekvens for eksisterende login token. Skjer ved første tilkobling til nettsiden.
```mermaid
sequenceDiagram
    actor User
    participant View(Index)
    participant LoginController
    participant LoginService
    participant UserDatabase

    User ->>+ View(Index): Enters Index View
    View(Index) ->>+ LoginService: Checks for existing token in cookies
    LoginService ->>+ UserDatabase: Validates token
    UserDatabase -->>- LoginService: Returns validation result
    LoginService -->>- View(Index): Sends validation status (valid/invalid)
    alt Token is valid
        View(Index) -->> User: e
    else Token is invalid or absent
        View(Index) -->>+ User: f
    end
```

3. Sekvens for login hvor token ikke eksisterer.
```mermaid
sequenceDiagram
    actor User
    participant View(Index)
    participant View(Login)
    participant LoginController
    participant LoginService
    participant UserDatabase

    User ->>+ View(Index): Clicks "Log in" button
    View(Index) ->>+ View(Login): Redirects to Login View
    View(Login) ->>+ LoginController: Submits username and password /POST
    LoginController ->>+ LoginService: Requests validation from service
    LoginService ->>+ UserDatabase: Validates credentials, password is hashed from FormData
    UserDatabase -->>- LoginService: Returns validity of credentials
    alt Credentials are valid
        LoginService -->> LoginController: Returns success with token
        LoginController -->> View(Login): Responds with success and token
        View(Login) -->> User: Displays success message and authenticated view
    else Credentials are invalid
        LoginService -->>- LoginController: Returns error
        LoginController -->>- View(Login): Responds with error
        View(Login) -->>- User: Displays error feedback
    end
```

4. Sekvens for å lage en ny bruker.
```mermaid
sequenceDiagram
    actor AnonymousUser
    participant LoginController
    participant LoginService
    participant UserDatabase

    AnonymousUser ->>+ LoginController: /POST /new (FormData)
    LoginController ->>+ LoginService: Enforce invariants, hash sensitive fields
    alt validation succeeds
        LoginService ->> UserDataBase: Insert new hashed user data into the database
        UserDatabase -->> LoginService: OK
        LoginService -->> LoginController: Success response
        LoginController -->> AnonymousUser: HTTP 201 Created
    else validation fails
        LoginService -->>- LoginController: Validation error
        LoginController -->- AnonymousUser: HTTP 400 Bad Request
    end
```

5. Sekvens for å hente innleggdata til hovedsiden når en bruker entrer DefaultRoute.
```mermaid
sequenceDiagram
    actor User
    participant BlogPostController
    participant DatabaseContext
    participant BlogPostDatabase

    User ->>+ BlogPostController: /GET
    BlogPostController ->>+ DatabaseContext: Begin fetching of published blog posts
    DatabaseContext ->>+ BlogPostDatabase: Fetch blog posts marked as public
    BlogPostDatabase -->- DatabaseContext: Return public blog posts

    alt User is authenticated
        BlogPostController ->> DatabaseContext: Begin fetching of blog posts associated with user
        DatabaseContext ->> BlogPostDatabase: Fetch events where user is owner
        DatabaseContext ->> BlogPostDatabase: Fetch events where user is author
        BlogPostDatabase -->> DatabaseContext: return owned blog posts
        BlogPostDatabase -->> DatabaseContext: return co-authored posts
        DatabaseContext -->> BlogPostController: Combined both published and user associated posts
        BlogPostController -->> User: Return JSON serialised DTO objects of published and user associated posts
    else User is anonymous
        DatabaseContext -->>- BlogPostController: Return published posts only
        BlogPostController -->- User: Return JSON serialised DTO objects of published posts
    end
```

6. Sekvens for å lage ett nytt innlegg
```mermaid
sequenceDiagram
    actor User
    participant BlogPostController
    participant DTOConstructor
    participant DatabaseContext
    participant BlogPostDatabase

    User ->>+ BlogPostController: /POST /json-serialised data
    BlogPostController ->>+ DTOConstructor: Validate and construct DTO from json-serialised data
    alt DTO validation succeeds
        DTOConstructor ->> DatabaseContext: Pass DTO with User as Owner
        DatabaseContext ->> BlogPostDatabase: Insert blog post into database
        DatabaseContext ->> BlogPostDatabase: Update relation table, Owner <=> Post
        BlogPostDatabase -->> DatabaseContext: OK (BlogPost created)
        BlogPostDatabase -->> DatabaseContext: OK (Relation updated)
        DatabaseContext -->> BlogPostController: Success response
        BlogPostController -->> User: HTTP 201 Created with id and redirect to new post
    else DTO validation fails
        DTOConstructor -->- BlogPostController: Validation error
        BlogPostController -->- User: HTTP 400 Bad Request with error message
    end
```

7. Sekvens for å redigere ett innlegg.
```mermaid
sequenceDiagram
    actor User
    participant BlogPostController
    participant AuthorisationService
    participant DTOConstructor
    participant DatabaseContext
    participant BlogPostDatabase

    User ->>+ BlogPostController: /PATCH /blog/{id} json-serialised data
    BlogPostController ->>+ AuthorisationService: Is the user allowed to perform this operation
    alt User has editing privileges
        AuthorisationService -->> BlogPostController: Authorised
        BlogPostController ->> DTOConstructor: Validate and construct DTO from json-serialised data
        alt DTO validation succeeds
            DTOConstructor ->> DatabaseContext: Pass DTO and post ID for update
            DatabaseContext ->> BlogPostDatabase: Update event with new data
            BlogPostDatabase -->> DatabaseContext: OK (BlogPost updated)
            DatabaseContext -->> BlogPostController: Success response
            BlogPostController -->> User: HTTP 200 OK with updated content
        else DTO validation fails
            DTOConstructor -->> BlogPostController: Validation Error
            BlogPostController -->> User: HTTP 400 Bad Request with error message
        end 
    else User lacks priviliges
        AuthorisationService -->>- BlogPostController: Validation Error
        BlogPostController -->>- User: HTTP 403 Forbidden with error message
    end
```

8. Sekvens for å slette ett innlegg.
```mermaid
sequenceDiagram
    actor User
    participant BlogPostController
    participant AuthorisationService
    participant DatabaseContext
    participant BlogPostDatabase
    participant RecordScrubbingDatabase
    actor RecordScrubberService

    User ->>+ BlogPostController: /DELETE /blog/{id}
    BlogPostController ->>+ AuthorisationService: Check if user is the post's owner
    alt User has deletion privilege
        AuthorisationService ->> BlogPostController: Authorised
        BlogPostController ->>+ DatabaseContext: Mark post for deletion
        DatabaseContext ->>+ BlogPostDatabase: Unpublish blog post
        BlogPostDatabase -->>- DatabaseContext: OK unpublished
        DatabaseContext ->>+ RecordScrubbingDatabase: Queue new job with a time in the future
        RecordScrubbingDatabase -->>- DatabaseContext: OK job created
        DatabaseContext -->>- BlogPostController: Return success response
        BlogPostController -->> User: HTTP 204 No Content
    else User lacks privileges
        AuthorisationService -->>- BlogPostController: Unauthorised
        BlogPostController -->>- User: HTTP 403 Forbidden with error message
    end
    RecordScrubberService ->>+ DatabaseContext: Periodically fetch queue
    DatabaseContext ->> RecordScrubbingDatabase: Fetch jobs whose execution date is now or in the past
    RecordScrubbingDatabase -->> DatabaseContext: Return jobs to be executed
    DatabaseContext ->>+ BlogPostDatabase: Delete record
    BlogPostDatabase -->>- DatabaseContext: OK
    DatabaseContext ->>+ RecordScrubbingDatabase: Clear job
    RecordScrubbingDatabase -->>- DatabaseContext: OK
    DatabaseContext -->>- RecordScrubberService: OK response (records and relations deleted)
```

## Entitetsrelasjonsdiagram

```mermaid
erDiagram
    User {
        uuid UserId
        string UserName
        string HashPassword
        set OwnedPosts
        Set AuthoredPosts
    }
    BlogPost {
        uuid BlogId
        string BlogTitle
        string BlogBody
        bool Published
        uuid OwnerId
        set Authors
        i64 views
    }
    User ||--o{ BlogPost: owner
    User }|--o{ BlogPost: author
```

Det skal også lages relasjonstabeller som holder oversikt over hvilken bruker
som har en relasjon til hvilken blogpost.
  - User 1..n UserOwnerRelation 1..1 BlogPost
  - User 1..n UserAuthorRelation n..m BlogPost