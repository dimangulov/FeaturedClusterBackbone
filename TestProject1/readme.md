- map REST API request to generic CadRequest
- pass generic cad request to the GRPC service
- map the generic GRPC cad request to executing method?

Tests
- REST request to generic request
- Generic request to a specific GRPC request

Can we have only 2 models
- REST model
- GRPC model

Should local requests be done using GRPC or local calls instead?
- the improvement can be done in V1.1