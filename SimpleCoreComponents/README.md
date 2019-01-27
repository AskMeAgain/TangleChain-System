# Simple Components

This project contains the most basic components for the IXICore to  run.
It is planned later on to release more streamlined components (eg caching and stuff like that).
An architectur like this provides me the ability to develop the stuff later on and users can
user their own implementation of specific things. Example:

Currently there is SimpleTangleAccessor implemented which connects to a node and
requests a block. You could implement a TangleConnector to connect to your local host, or you could implement
a caching system inbetween so you dont have to request always the same block again (VIP)

You need to install a component or else you cant use IXICore. 

This project provides an extension method to instantiate IXICore

    var ixiCore = (null as IXICore).SimpleSetup("CoinName",new IXISettings());