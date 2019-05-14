# ConsulDemo
Consul demo app for Microservice, service discovery using [Consul.Net](https://github.com/PlayFab/consuldotnet)

You will need to grab the following before you start

# Consul download

[Consul downloads](https://www.consul.io/downloads.html)

And ensure that this is installed/extracted somewhere and that Consul.exe is on your path

# Running the demo

## Step 1 : Start consul


The following command is enough in most cases to run a single node consul environment
```
consul agent -dev
```

If you have multiple IPs on your machine (usually the case if you have VM software setup or multiple network adapters) then
the previous command might have failed. Instead, you'll have to specify the IP you wish for Consul to listen on. The following
startup options might help.

```
consul agent -dev --node yourNodeName --bind 11.11.11.11 -client 11.11.11.11 -ui
```

Check the web API  [http://127.0.0.1:8500](http://127.0.0.1:8500)


## Step 2 : Open the solution and run these in order
- ConsulDemoApi
- ConsulDemoApi.Client

## Step 3 : Read the full article 
Read the full article that describes this here [https://www.codeproject.com/Articles/1248381/Microservices-Service-Discovery](https://www.codeproject.com/Articles/1248381/Microservices-Service-Discovery)


