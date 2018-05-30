# TangleChain

TangleChain is a .Net Standard Library & IXI module to deploy different blockchains on top of the Tangle. It is planned to have Smartcontracts and interoperability between different chains. The planned blocktime is around 1-2 Minutes.


### Current Features

- Complete on tangle blockchain
- local database storage
- longest chain detection
- Easy start of a chain
- Proof-of-Work consensus in a transition phase

### Missing Features

- Difficulty System.
- Blocks are not correctly verified (balance)
- No Account System (public private key)
- Interoperability between different chains
- Smart contracts
- Environment friendly consensus protocol
- Complete Documentation
- Nuget Package


## Getting Started

The best way to get started is to check the unit tests. Checkout the example classes in the Example Folder for a general idea.

#### Settings

Most of the functions need some settings to work. For example when Finalizing a Block/Transaction you need to set your public key.

#### How to Mine a Block

- Create a new Block
- Fill block with:
 - Transaction hashes via AddTransactions()
 - Owner Public Key
 - Block Height
 - Coin Name
- Add Block Hash via GenerateHash()
- Sign Block via Sign()
- Upload Block via Core.UploadBlock()

PS: You need to check if the Transaction got included in a block before. If yes, then the Block is invalid and will be rejected from the Nodes.

#### How to Send a AddTransactions

- Create a new Transactionpool
- Add a TransactionFee via AddFee()
- Add Outputs via AddOutput()
- Set Identity.SendTo to the correct TransactionPoolAddress
- Set Hash & Sign the transaction via Finalize()
- upload Transaction via Core.UploadTransaction();


#### Consensus

TODO, how is a block structured, TransactionPoolAddress, when is a block invalid etc.
