﻿using System;
using System.Collections.Generic;
using Blockcore.Features.Consensus.Rules.CommonRules;
using Blockcore.Features.Consensus.Rules.ProvenHeaderRules;
using Blockcore.Features.Consensus.Rules.UtxosetRules;
using Blockcore.Features.MemoryPool.Rules;
using BlockcoreSampleCoin.Networks.Deployments;
using BlockcoreSampleCoin.Networks.Policies;
using BlockcoreSampleCoin.Networks.Rules;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;

namespace BlockcoreSampleCoin.Networks
{
    // TODO: rename networks and other "bsc" strings
    public class BscMain : Network
    {
        /// <summary> The name of the root folder containing the different BlockcoreSampleCoin blockchains (BscMain, BscTest, BscRegTest). </summary>
        public const string BscRootFolderName = "bsc";

        /// <summary> The default name used for the BlockcoreSampleCoin configuration file. </summary>
        public const string BscDefaultConfigFilename = "bsc.conf";

        public BscMain()
        {
            // TODO: define your magic number
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0x70;
            messageStart[1] = 0x35;
            messageStart[2] = 0x22;
            messageStart[3] = 0x05;
            uint magic = BitConverter.ToUInt32(messageStart, 0); //0x5223570;

            this.Name = "BscMain";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = magic;

            // TODO: set your ticker
            this.CoinTicker = "BSC";

            // TODO: set your ports and defaults
            this.DefaultPort = 16178;
            this.DefaultRPCPort = 16174;
            this.DefaultAPIPort = 37221;
            this.DefaultSignalRPort = 38824;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = BscRootFolderName;
            this.DefaultConfigFilename = BscDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.DefaultBanTimeSeconds = 16000; // 500 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1470467000;
            this.GenesisNonce = 1831645;
            this.GenesisBits = 0x1e0fffff;
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            Block genesisBlock = CreateGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward);

            this.Genesis = genesisBlock;

            // Taken from StratisX.
            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5,
                witnessScaleFactor: 4
            );

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new BscBIP9Deployments()
            {
                [BscBIP9Deployments.TestDummy] = new BIP9DeploymentsParameters("TestDummy", 28,
                    new DateTime(2019, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2020, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    BIP9DeploymentsParameters.DefaultMainnetThreshold),

                [BscBIP9Deployments.CSV] = new BIP9DeploymentsParameters("CSV", 0,
                    new DateTime(2019, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2020, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    BIP9DeploymentsParameters.DefaultMainnetThreshold),

                [BscBIP9Deployments.Segwit] = new BIP9DeploymentsParameters("Segwit", 1,
                    new DateTime(2019, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2020, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    BIP9DeploymentsParameters.DefaultMainnetThreshold),

                [BscBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters("ColdStaking", 2,
                    new DateTime(2019, 12, 2, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2020, 12, 2, 0, 0, 0, DateTimeKind.Utc),
                    BIP9DeploymentsParameters.DefaultMainnetThreshold)
            };

            this.Consensus = new NBitcoin.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 105,
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: new uint256("0x6ad909469bc8fa15f48533fd30ae3217fceca413c0df69cf4ac314188f4df1c4"), // 1600000
                maxMoney: long.MaxValue,
                coinbaseMaturity: 50,
                premineHeight: 2,
                premineReward: Money.Coins(98000000),
                proofOfWorkReward: Money.Coins(4),
                targetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                targetSpacing: TimeSpan.FromSeconds(64),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 12500,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.COIN
            );

            this.Consensus.PosEmptyCoinbase = true;

            // TODO: Set your Base58Prefixes
            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (63) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (125) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (63 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
            };

            this.Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder("bsc");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.DNSSeeds = new List<DNSSeedData>
            {
                // TODO: Add DNS seeds here
                // new DNSSeedData("X.bsc.com", "X.bsc.com"),
            };

            this.SeedNodes = new List<NetworkAddress>
            {
                // TODO: Add seed nodes here
                // new NetworkAddress(IPAddress.Parse("X.X.X.X"), 16178), 
            };

            this.StandardScriptsRegistry = new BscStandardScriptsRegistry();

            // 64 below should be changed to TargetSpacingSeconds when we move that field.
            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * 64 / 2);

            // TODO: update RHS to match HashGenesisBlock & HashMerkleRoot
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x0000066e91e46e5a264d42c89e1204963b2ee6be230b443e9159020539d972af"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0x65a26bc20b0351aebf05829daefa8f7db2f800623439f3c114257c91447f1518"));

            this.RegisterRules(this.Consensus);
            this.RegisterMempoolRules(this.Consensus);
        }

        protected void RegisterRules(IConsensus consensus)
        {
            consensus.ConsensusRules
                .Register<HeaderTimeChecksRule>()
                .Register<HeaderTimeChecksPosRule>()
                .Register<BscPosFutureDriftRule>()
                .Register<CheckDifficultyPosRule>()
                .Register<BscHeaderVersionRule>()
                .Register<ProvenHeaderSizeRule>()
                .Register<ProvenHeaderCoinstakeRule>();

            consensus.ConsensusRules
                .Register<BlockMerkleRootRule>()
                .Register<PosBlockSignatureRepresentationRule>()
                .Register<PosBlockSignatureRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsPartialValidationRule>()
                .Register<PosTimeMaskRule>()

                // rules that are inside the method ContextualCheckBlock
                .Register<TransactionLocktimeActivationRule>()
                .Register<CoinbaseHeightActivationRule>()
                .Register<WitnessCommitmentsRule>()
                .Register<BlockSizeRule>()

                // rules that are inside the method CheckBlock
                .Register<EnsureCoinbaseRule>()
                .Register<CheckPowTransactionRule>()
                .Register<CheckPosTransactionRule>()
                .Register<CheckSigOpsRule>()
                .Register<PosCoinstakeRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsFullValidationRule>()

                .Register<CheckDifficultyHybridRule>()

                // rules that require the store to be loaded (coinview)
                .Register<FetchUtxosetRule>()
                .Register<TransactionDuplicationActivationRule>()
                .Register<CheckPosUtxosetRule>() // implements BIP68, MaxSigOps and BlockReward calculation
                                                 // Place the PosColdStakingRule after the PosCoinviewRule to ensure that all input scripts have been evaluated
                                                 // and that the "IsColdCoinStake" flag would have been set by the OP_CHECKCOLDSTAKEVERIFY opcode if applicable.
                .Register<PosColdStakingRule>()
                .Register<PushUtxosetRule>();
        }

        protected void RegisterMempoolRules(IConsensus consensus)
        {
            consensus.MempoolRules = new List<Type>()
            {
                typeof(CheckConflictsMempoolRule),
                typeof(CheckCoinViewMempoolRule),
                typeof(CreateMempoolEntryMempoolRule),
                typeof(CheckSigOpsMempoolRule),
                typeof(CheckFeeMempoolRule),
                typeof(CheckRateLimitMempoolRule),
                typeof(CheckAncestorsMempoolRule),
                typeof(CheckReplacementMempoolRule),
                typeof(CheckAllInputsMempoolRule),
                typeof(CheckTxOutDustRule)
            };
        }

        protected static Block CreateGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            // TODO: configure genesis block
            string pszTimestamp = "PUT UNIQUE STRING HERE";

            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 1;
            if (txNew is IPosTransactionWithTime posTx)
                posTx.Time = nTime;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(0), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)42 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(pszTimestamp)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });

            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = nVersion;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();
            return genesis;
        }
    }
}