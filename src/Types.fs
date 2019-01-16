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
    | DeleteWithTag of repo:string * tag:string
    | ImageDigestFetched of string
    | CatelogFetched of Catelog
    | RepositoryTagsFetched of RepositoryTags
    | FetchError of exn
