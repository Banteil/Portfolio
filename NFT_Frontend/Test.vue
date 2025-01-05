<template>
  <q-page class="page-1200 project-list-wrap">
    <div class="row q-pt-md q-pl-md justify-center page-tit">
      <div class="col-12 doc-heading doc-h2">
        {{ $t("test") }}
      </div>
    </div>
    <div class="row q-pl-md row justify-center page-sub-tit">
      <div class="col-12">
        {{ $t("test..") }}
      </div>
    </div>

    <!-- 솔라나 주소 입력 및 유효성 검사 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>{{ $t("Enter Solana address") }}</label>
        <q-input
          v-model="solanaAddress"
          outlined
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          @click="checkAddress()"
        >
          {{ $t("Check Address") }}
        </q-btn>
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div v-if="!isSolanaAddressValid" class="text-red">
        Invalid Solana address
      </div>
      <div v-if="isSolanaAddressValid" class="text-green">
        Valid Solana address
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 민팅 데이터 입력 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-12 text-white">
        <h3>Minting Data</h3>
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-white">
        <label>{{ $t("NFT Name") }}</label>
        <q-input
          v-model="nftData.name"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("NFT Symbol") }}</label>
        <q-input
          v-model="nftData.symbol"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div class="col-6 text-white">
        <label>{{ $t("NFT URI") }}</label>
        <q-input
          v-model="nftData.uri"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("Seller Fee Basis Points") }}</label>
        <q-input
          v-model="nftData.sellerFeeBasisPoints"
          type="number"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div class="col-6 text-white">
        <label>{{ $t("Token Owner Address") }}</label>
        <q-input
          v-model="nftData.tokenOwner"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("Collection Mint Address") }}</label>
        <q-input
          v-model="nftData.collectionMint"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-left">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          style="width: 98%"
          @click="mintSolanaNftTest()"
        >
          {{ $t("Mint Solana NFT") }}
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 컬렉션 데이터 입력 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-12 text-white">
        <h3>Collection Data</h3>
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-white">
        <label>{{ $t("Collection Name") }}</label>
        <q-input
          v-model="collectionData.name"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("Collection Symbol") }}</label>
        <q-input
          v-model="collectionData.symbol"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div class="col-12 text-white">
        <label>{{ $t("Collection URI") }}</label>
        <q-input
          v-model="collectionData.uri"
          outlined
        />
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-left">
        <q-btn
          class="btn"
          color="secondary"
          text-color="black"
          size="lg"
          style="width: 98%"
          @click="mintSolanaCollection()"
        >
          {{ $t("Mint Solana Collection") }}
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 에어드랍 기능 추가 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-12 text-white">
        <h3>Transfer Solana NFT</h3>
      </div>
    </div>
    <div class="row justify-center q-pt-lg">
      <div class="col-6 text-white">
        <label>{{ $t("Recipient Wallet Address") }}</label>
        <q-input
          v-model="solanaTransferAddress"
          outlined
        />
      </div>
      <div class="col-6 text-white">
        <label>{{ $t("NFT Mint Address to Transfer") }}</label>
        <q-input
          v-model="solanaTransferMintAddress"
          outlined
        />
      </div>
      <div class="col-6 text-left">
        <q-btn
          class="btn"
          color="warning"
          text-color="black"
          size="lg"
          style="width: 98%"
          @click="transferSolanaNft()"
        >
          {{ $t("Transfer Solana NFT") }}
        </q-btn>
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>

    <!-- 이더리움 주소 입력 및 유효성 검사 -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>{{ $t("Enter Ethereum address") }}</label>
        <q-input
          v-model="ethereumAddress"
          outlined
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          @click="checkEthereumAddress()"
        >
          {{ $t("Check Address") }}
        </q-btn>
      </div>
    </div>
    <div class="row justify-center q-pt-md">
      <div v-if="!isEthereumAddressValid" class="text-red">
        Invalid Ethereum address
      </div>
      <div v-if="isEthereumAddressValid" class="text-green">
        Valid Ethereum address
      </div>
    </div>

    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>
    
    <!-- Wallet Address Input -->
    <div class="row justify-center q-pt-lg">
      <div class="col-8 text-white">
        <label>{{ $t("Enter Wallet Address") }}</label>
        <q-input
          v-model="walletAddress"
          outlined
        />
      </div>
      <div class="col-4">
        <q-btn
          class="btn"
          color="primary"
          text-color="black"
          size="lg"
          @click="fetchEthereumNftList()"
        >
          {{ $t("Fetch NFTs") }}
        </q-btn>
      </div>
      <div class="text-green">
        Get nft list
      </div>
    </div>
    <div class="text-white q-pa-xl">
      <h1>사용자가 소유한 NFT 목록</h1>
      <div>
        <input type="checkbox" v-model="selectAll" @change="toggleSelectAll" /> 전체 선택 / 취소
      </div>
      <div v-if="ownNftList.length > 0">
        <div v-for="(nft, index) in ownNftList" :key="index" class="nft-item q-pa-xl">
          <input type="checkbox" v-model="nft.selected" />
          <img :src="nft.metadata.image" :alt="nft.title" class="nft-image" style="width: 100px; height: auto;"/>
          <h2>nft title: {{ nft.title }}</h2>
          <p>description: {{ nft.description }}</p>
          <p>contract name: {{ nft.contractMetadata.name }}</p>
          <p>token ID: {{ nft.id.tokenId }}</p>
          <p>balance: {{ nft.balance }}</p>
        </div>
      </div>
      <div v-else>
        <p>소유한 NFT가 없습니다.</p>
      </div>
    </div>



    
    <!-- 하단 공간 확보 -->
    <div class="row justify-center q-pa-xl"></div>
  </q-page>
</template>

<script>
import { defineComponent } from "vue";
import { useI18n } from "vue-i18n";
import { PublicKey, Keypair, Connection } from "@solana/web3.js";
import { getOrCreateAssociatedTokenAccount, transfer, TOKEN_PROGRAM_ID } from "@solana/spl-token";
import bs58 from 'bs58';
import { createUmi } from "@metaplex-foundation/umi-bundle-defaults";
import { generateSigner, keypairIdentity, percentAmount } from "@metaplex-foundation/umi";
import { createNft, mplTokenMetadata, verifyCollectionV1, fetchDigitalAsset } from "@metaplex-foundation/mpl-token-metadata";
import Web3 from 'web3';

export default defineComponent({
  name: "Test",
  setup() {
    const { locale } = useI18n({ useScope: "global" });
    return {
      locale,
    };
  },
  data() {
    return {
      solanaAddress: '',
      solanaTransferAddress: '8zni7y1DzB7jq7ym9iRLXLwPaohs8Z4bPTTo6bw9XaKo',
      solanaTransferMintAddress: '',
      walletAddress: '0x5f1DBe428EBAb9A9aA724aBd0Fff3d6f64E79172',
      contractAddress: '0x46FdfCb3Cd89A1C54D36EE83a4ADC184747B40D9',
      ownNftList: [],
      selectAll: false,
      isSolanaAddressValid: true,
      ethereumAddress: '',
      isEthereumAddressValid: true,
      // solanaNetwork: "https://api.devnet.solana.com",
      solanaNetwork: "https://quaint-thrumming-mountain.solana-mainnet.quiknode.pro/19adaf15ef3c5a0ae692d1ccec0e47244f261d2a",
      payerSecretKey: '',
      nftData: {
        name: "TEST NFT",
        symbol: "TT",
        uri: "https://arweave.net/tx0uyd4lDGPvZIUMBgz2-jJEdTBMCky8kqxu06dufM4",
        sellerFeeBasisPoints: 7.5,
        tokenOwner: "4gbdX7szo3RN4nNmc8BFM6buAkfu9sKhHSbp1uo4CLUF",
        collectionMint: "Ejf1fuuzXXj8LDpKkJYRoeetnEFqRGY923Tft33TTGkY",
      },
      collectionData: {
        name: "TTEST Collection",
        symbol: "BT",
        uri: "https://arweave.net/tx0uyd4lDGPvZIUMBgz2-jJEdTBMCky8kqxu06dufM4",
        sellerFeeBasisPoints: 0,
      },
    };
  },
  created: function () {
    // 키 벨류 조회
    this.selectKeyValue()
  },
  methods: {
    async checkAddress() {
      this.isSolanaAddressValid = await this.isValidSolanaAddress(this.solanaAddress);
      console.log(this.isSolanaAddressValid ? 'Valid Solana address' : 'Invalid Solana address');
    },
    async checkEthereumAddress() {
      const web3 = new Web3();
      this.isEthereumAddressValid = web3.utils.isAddress(this.ethereumAddress);
      console.log(this.isEthereumAddressValid ? 'Valid Ethereum address' : 'Invalid Ethereum address');
    },
    async selectKeyValue() {
      const paramKeyValuePayerSecretKey = {
        cdKey: this.$KEY_VALUE_SOLANA_PAYER_SECRET_KEY,
      }
      const resultKeyValuePayerSecretKey = await this.$axios.get('/api/common/selectKeyValue', { params: { ...paramKeyValuePayerSecretKey }})
      // console.log(resultKeyValue.data)
      if (resultKeyValuePayerSecretKey.data) {
        this.payerSecretKey = resultKeyValuePayerSecretKey.data.returnValue
      }

      const paramKeyValueSellerFeeBasisPoint = {
        cdKey: this.$KEY_VALUE_SOLANA_SELLER_FEE_BASIS_POINT,
      }
      const resultKeyValueSellerFeeBasisPoint = await this.$axios.get('/api/common/selectKeyValue', { params: { ...paramKeyValueSellerFeeBasisPoint }})
      // console.log(resultKeyValue.data)
      if (resultKeyValueSellerFeeBasisPoint.data) {
        this.collectionData.sellerFeeBasisPoints = resultKeyValueSellerFeeBasisPoint.data.returnValue
        this.nftData.sellerFeeBasisPoints = resultKeyValueSellerFeeBasisPoint.data.returnValue
      }

      // const paramKeyValueSymbol = {
      //   cdKey: this.$KEY_VALUE_SOLANA_SYMBOL,
      // }
      // const resultKeyValueSymbol = await this.$axios.get('/api/common/selectKeyValue', { params: { ...paramKeyValueSymbol }})
      // // console.log(resultKeyValue.data)
      // if (resultKeyValueSymbol.data) {
      //   // this.collectionData.symbol = resultKeyValueSymbol.data.returnValue
      // }
    },
    async mintSolanaNftTest() {
      try {
        console.log("■■■■■■■ mintSolanaNft Test ■■■■■■■ START");
        const collectionKey = new PublicKey(this.nftData.collectionMint);
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());
        const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(payer));
        const tokenOwner = new PublicKey(this.nftData.tokenOwner);

        const mint = generateSigner(umi);
        console.log("NFT mint account : ", mint.publicKey);
        console.log("Payer account : ", payer.publicKey);

        console.log("Start Create NFT");
        const createdNft = await createNft(umi, {
          mint,
          name: this.nftData.name,
          symbol: this.nftData.symbol,
          uri: this.nftData.uri,
          sellerFeeBasisPoints: percentAmount(this.nftData.sellerFeeBasisPoints * 0.01),
          collection: {
            verified: false,
            key: collectionKey,
          },
          creators: [
            {
              address: payer.publicKey,
              verified: true,
              share: 100,
            },
          ],
          authority: payer.publicKey,
          tokenOwner: tokenOwner,
          isCollection: false,
        }).sendAndConfirm(umi);
        console.log("End Create NFT");

        console.log("Start Verify Collection");
        const asset = await this.fetchAssetWithRetries(umi, mint.publicKey) 
        //콜렉션 검증
        const metadata = asset.metadata
        const verify = await verifyCollectionV1(umi, {
          metadata,
          collectionMint: collectionKey,
          authority: payer.publicKey,
        }).sendAndConfirm(umi)

        //컬렉션 검증 트랜잭션 상태 확인
        if (verify.result.value.err !== null) {
          throw new Error(`Verification failed with error: ${verify.result.value.err}`)
        }
        console.log(
          "NFT Verify Collection Signature : ",
          bs58.encode(verify.signature)
        )
        console.log("End Verify Collection");

        console.log("Create NFT Signature : ", bs58.encode(createdNft.signature));
        console.log("■■■■■■■ mintSolanaNft Test ■■■■■■■ END");
      } catch (error) {
        console.error("Error minting and verifying Solana NFT:", error);
      }
    },
    async mintSolanaCollection() {
      try {
        console.log("■■■■■■■ mintSolanaCollection Test ■■■■■■■ START");
        const umi = createUmi(this.$SOLANA_NETWORK).use(mplTokenMetadata());
        const payer = umi.eddsa.createKeypairFromSecretKey(bs58.decode(this.payerSecretKey));
        umi.use(keypairIdentity(payer));

        const collectionMint = generateSigner(umi);
        console.log("Collection mint account : ", collectionMint.publicKey);    

        const createdCollection = await createNft(umi, {
          mint: collectionMint,
          name: this.collectionData.name,
          symbol: this.collectionData.symbol,
          uri: this.collectionData.uri,
          sellerFeeBasisPoints: percentAmount(this.collectionData.sellerFeeBasisPoints * 0.01),
          creators: [
            {
              address: payer.publicKey,
              verified: true,
              share: 100,
            },
          ],
          authority: payer.publicKey,
          tokenOwner: payer.publicKey,
          isCollection: true,
        }).sendAndConfirm(umi);

        console.log(
          "Create Collection Signature : ",
          bs58.encode(createdCollection.signature)
        );
        console.log("■■■■■■■ mintSolanaCollection Test ■■■■■■■ END");
      } catch (error) {
        console.error("Error minting Solana Collection:", error);
      }
    },
    async transferSolanaNft() {
      try {
        console.log("■■■■■■■ transferSolanaNft Test ■■■■■■■ START");
        const connection = new Connection(this.$SOLANA_NETWORK, 'confirmed');
        const payer = Keypair.fromSecretKey(bs58.decode(this.payerSecretKey));
        const mintPublicKey = new PublicKey(this.solanaTransferMintAddress);
        const recipientPublicKey = new PublicKey(this.solanaTransferAddress);

        console.log("mintPublicKey : ", this.solanaTransferMintAddress);
        console.log("recipientPublicKey : ", this.solanaTransferAddress);

        const payerTokenAccount = await getOrCreateAssociatedTokenAccount(
          connection,
          payer,
          mintPublicKey,
          payer.publicKey
        );
        const recipientTokenAccount = await getOrCreateAssociatedTokenAccount(
          connection,
          payer,
          mintPublicKey,
          recipientPublicKey
        );

        // Transfer the token
        const signature = await transfer(
          connection,
          payer,
          payerTokenAccount.address,
          recipientTokenAccount.address,
          payer,
          1, // Amount to transfer (for NFTs, typically 1)
          [],
          TOKEN_PROGRAM_ID
        );

        console.log("Transfer NFT Signature : ", signature);
        console.log("■■■■■■■ transferSolanaNft Test ■■■■■■■ END");
      } catch (error) {
        console.error("Error transferring Solana NFT:", error);
      }
    },
    // async fetchEthereumNftList() {
    //   try 
    //   {
    //     const provider = new ethers.providers.JsonRpcProvider('https://rpc.ankr.com/eth');
    //     console.log('provider : ', provider);
    //     const contract = new ethers.Contract(this.contractAddress, this.$ERC721_ABI, provider);
    //     console.log('contract : ', contract);

    //     // 사용자가 소유한 NFT의 총 개수를 가져옵니다.
    //     const balance = await contract.balanceOf(this.walletAddress);
    //     console.log(`Total NFTs owned: ${balance.toString()}`);
        
    //     if (balance > 0) {
    //       let ownedTokenIds = [];
    //       // 모든 토큰 ID를 순회하며 소유자가 일치하는 토큰을 가져옵니다.
    //       const totalSupply = await contract.totalSupply();
    //       for (let tokenId = 0; tokenId < totalSupply; tokenId++) {
    //         try {
    //           const owner = await contract.ownerOf(tokenId);
    //           if (owner.toLowerCase() === this.walletAddress.toLowerCase()) {
    //             ownedTokenIds.push(tokenId.toString());
    //             console.log(`Token ID owned by ${this.walletAddress}: ${tokenId}`);
    //           }
    //         } catch (error) {
    //           console.error(`Failed to fetch owner of token ID ${tokenId}:`, error);
    //         }
    //       }
    //       console.log('Owned Token IDs:', ownedTokenIds);
    //     } else {
    //       console.log('No NFTs owned by this address');
    //     }
    //   } catch (error) {
    //     console.error('Error fetching NFT details:', error);
    //     throw error;
    //   }
    // },    
    async fetchEthereumNftList() {
      const ownerAddress = this.walletAddress
      const contractAddress = this.contractAddress
      
      const apiKey = "wspRO3sE6afxI_nRqFoKLsz6sLDielYd"; // Alchemy API 키
      const url = `https://eth-mainnet.alchemyapi.io/v2/${apiKey}/getNFTs/`;

      try {
          const response = await this.$axios.get(url, {
              params: {
                  owner: ownerAddress,
                  contractAddresses: [contractAddress]
              }
          });

          console.log("사용자가 소유한 특정 컨트랙트의 NFT:", response.data)

          this.ownNftList = response.data.ownedNfts


          console.log('this.ownNftList')
          console.log(this.ownNftList)
      } catch (error) {
          console.error("NFT 조회 중 오류 발생:", error)
      }
    },    
    async isValidSolanaAddress(address) {
      try {
        let key = new PublicKey(address);
        return PublicKey.isOnCurve(key.toBuffer())
      } catch (error) {
        console.log('error : ', error)
        return false
      }
    },
    async fetchAssetWithRetries(umi, publicKey, retries = 1000, delay = 100) {
      for (let i = 0; i < retries; i++) {
        try {
          const asset = await fetchDigitalAsset(umi, publicKey)
          return asset
        } catch (error) {
          if (i < retries - 1) {
            await new Promise((resolve) => setTimeout(resolve, delay))
          }
        }
      }
      throw new Error("Failed to fetch asset after multiple attempts")
    },


    toggleSelectAll() {
      this.ownNftList.forEach(nft => {
        nft.selected = this.selectAll
      })
    },
  },
});
</script>

<style scoped></style>