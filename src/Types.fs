module Types



type Catelog = 
    { repositories : string list }

type RepositoryTags = 
    { name : string
      tags : string list }

type Manifest = 
    { schemaVersion : int
      name : string
      tag : string
      architecture : string
      fsLayers : FSLayer []
      digest : string }
and FSLayer = 
    { blobSum : string }      
and ImageHistory = 
    { v1Compatibility : V1Compatibility [] }
and V1Compatibility = 
    { id : string }

type Model =
    { Repositories : Repository list }

and Repository =
    { Name : string
      Tags : string list }

type Msg =
    | DeleteImage of name:string * tag:string
    | DeleteLayer of name:string * tag:string
    | ImageDeleted
    | ImageDigestFetched of name:string * digest:string
    | CatelogFetched of Catelog
    | RepositoryTagsFetched of RepositoryTags
    | FetchError of exn
