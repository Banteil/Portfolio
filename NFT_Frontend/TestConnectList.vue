<template>
    <q-page class="q-pa-md page-1200 q-page-mini">
        <div class="text-white">
          <div v-if="loading">로드 중...</div>
          <div v-else>
              <div v-if="nfts.length === 0">NFT가 없습니다.</div>
              <div v-else>
                  <div class="q-mb-md">
                      <q-btn label="전체 선택" @click="selectAllNFTs" color="primary" class="q-mr-sm" />
                      <q-btn label="선택 해제" @click="deselectAllNFTs" color="negative" class="q-mr-sm" />
                      <q-btn label="에어드랍 신청하기" @click="requestAirdrop" color="positive" />
                  </div>
                  <ul>
                      <li v-for="nft in nfts" :key="nft.tokenId">
                          <q-checkbox v-model="nft.selected" label="선택" />
                          <h3>{{ nft.name }}</h3>
                          <img :src="nft.image" alt="NFT 이미지" />
                          <p>{{ nft.description }}</p>
                          <!-- 하단 공간 확보 -->
                          <div class="row justify-center q-pa-xl"></div>
                      </li>
                  </ul>
              </div>
          </div>
          <ul>
            <li v-for="nft in nftIds" :key="nft.tokenId">
                <q-checkbox v-model="nft.selected" label="선택" />
                <img :src="nft.image_url" alt="NFT 이미지" style="width: 200px; height: auto;">
                <h3>{{ nft.tokenId }}</h3>
                <h3>{{ nft.name }}</h3>
                <p>{{ nft.tokenURI }}</p>
                <!-- 하단 공간 확보 -->
                <div class="row justify-center q-pa-xl"></div>
            </li>
          </ul>
        </div>
    </q-page>
</template>

<script>
import { defineComponent } from 'vue'
import { useI18n } from 'vue-i18n'
import { Connection, PublicKey } from "@solana/web3.js";
//BNB
import Web3 from 'web3';
import {ethers} from 'ethers'

export default defineComponent({
    name: 'PageIndex',
    data () {
      return {
        nftList: [],
        nfts: [],
        nftIds: [],
        loading: false,
        walletAddress: '0x83490CB4BE947c3F5d9d4465257339caab15d36f', // 여기에 사용자의 지갑 주소를 넣으세요.
        contractAddress: '0x46FdfCb3Cd89A1C54D36EE83a4ADC184747B40D9', // 여기에 특정 NFT 컨트랙트 주소를 넣으세요.
      }
    },
    components: {
    },
    watch: {
    },
    computed: {
      getWalletType () {
        return this.$store.getters.getWalletType
      },
      getWalletAddress () {
        return this.$store.getters.getWalletAddress
      },
      getWalletAddressShort () {
        const address = this.$store.getters.getWalletAddress
        if (address) {
          return address.substr(0, 6) + '...' + address.substr(address.length - 4, address.legnth)
        } else {
          return address
        }
      },
      getWalletAddressLast () {
        const address = this.$store.getters.getWalletAddress
        if (address) {
          return '...' + address.substr(address.length - 4, address.legnth)
        } else {
          return address
        }
      },
      getSolanaWalletType () {
        return this.$store.getters.getSolanaWalletType
      },
      // getSolanaWalletAddress () {
      //   return this.$store.getters.getSolanaWalletAddress
      // },
      // getSolanaWalletAddressLast () {
      //   const address = this.$store.getters.getSolanaWalletAddress
      //   if (address) {
      //     return '...' + address.substr(address.length - 4, address.legnth)
      //   } else {
      //     return address
      //   }
      // },
    //   isButtonActive () {
    //     const startTime = new Date(this.projectVo.mint_start_time)
    //     const endTime = new Date(this.projectVo.mint_end_time)
  
    //     // 현재 시간 가져오기
    //     const now = new Date()
        
    //     // 현재 시간이 설정한 범위 내에 있는지 확인
    //     return now >= startTime && now < endTime
    //   }
    },
    created: function () {
      // console.log(this.$q.platform.is.mobile)
      const walletAddress = localStorage.getItem('WALLETADDRESS') ? localStorage.getItem('WALLETADDRESS') : this.$cookie.get('walletAddress')
      const walletType = localStorage.getItem('WALLETTYPE') ? localStorage.getItem('WALLETTYPE') : this.$cookie.get('walletType')
      if (walletAddress && walletType) {
        this.$store.dispatch('setWalletType', walletType)
        this.$store.dispatch('setWalletAddress', walletAddress)
      }
  
      const solanaWalletAddress = localStorage.getItem('SOLANAWALLETADDRESS') ? localStorage.getItem('SOLANAWALLETADDRESS') : this.$cookie.get('solanaWalletAddress')
      const solanaWalletType = localStorage.getItem('SOLANAWALLETTYPE') ? localStorage.getItem('SOLANAWALLETTYPE') : this.$cookie.get('solanaWalletType')
      if (solanaWalletAddress && solanaWalletType) {
        this.$store.dispatch('setSolanaWalletType', solanaWalletType)
        this.$store.dispatch('setSolanaWalletAddress', solanaWalletAddress)
      }

      // this.fetchNFTs()
    },
    destroy: function () {
    },
    mounted: function () {
    //   if (this.$q.platform.is.desktop) {
    //     if (window.ethereum) {
    //       // 지갑주소 변경 이벤트 구독
    //       window.ethereum?.on('accountsChanged', this.walletAccountsChangedMetamask)
    //       // 체인 변경 이벤트 구독
    //       window.ethereum?.on('chainChanged', this.handleChainChangedMetamask)
    //     }
    //   }
        // this.fetchUserNFTs()
        // this.fetchNFTs()

        // Vue 컴포넌트가 마운트될 때 web3 초기화 및 NFT 목록을 가져옴
        if (typeof window.ethereum !== 'undefined') {
          this.$web3 = new Web3(window.ethereum); // MetaMask가 연결된 web3
          this.getNftListOfOwner();
        } else {
          console.error('MetaMask가 설치되지 않았습니다.');
        }
    },
    setup () {
      const { locale } = useI18n({ useScope: 'global' })
  
      return {
        locale,
      }
    },
    methods: {
        async fetchNFTs() {
            // 지갑 연결 체크
            if (!this.getWalletType || !this.getWalletAddress) {
                // connect metamask 안한 경우
                // this.connectMetaMaskWallet()
                this.$noti(this.$q, this.$t('parameter_check_failed_wallet_metamask'))
                return
            }

            this.loading = true
            try {
                const infuraApiKey = '960c2fe77d1d48f6b4429cd20db6432f' // Infura API 키
                // const provider = new ethers.providers.JsonRpcProvider(`https://mainnet.infura.io/v3/${infuraApiKey}`)
                // const provider = new ethers.providers.JsonRpcProvider(`https://rpc.ankr.com/eth`)
                const provider = new ethers.providers.JsonRpcProvider(`https://sepolia.infura.io/v3/${infuraApiKey}`)
                // const provider = new ethers.providers.JsonRpcProvider('http://127.0.0.1:8545')  // eth공식 네트워크?  ==> 사용안됨..
                const contractAbi = [
                    "function tokenURI(uint256 tokenId) view returns (string)",
                    "function ownerOf(uint256 tokenId) view returns (address)",
                    "function balanceOf(address owner) view returns (uint256)"
                ]
                const contract = new ethers.Contract(this.contractAddress, contractAbi, provider)

                // 사용자가 소유한 NFT의 총 개수
                const balance = await contract.balanceOf(this.walletAddress)

                console.log(balance)

                // 결과를 저장할 배열 초기화
                const nftList = []

                // 각 토큰의 ID를 가져와서 메타데이터를 조회
                for (let tokenId = 1; tokenId <= balance; tokenId++) {
                    try {
                        // 소유자가 현재 사용자의 지갑인지 확인
                        const owner = await contract.ownerOf(tokenId)
                        if (owner.toLowerCase() !== this.walletAddress.toLowerCase()) {
                            continue
                        }

                        const tokenURI = await contract.tokenURI(tokenId)

                        // tokenURI를 요청하고 JSON 형식의 메타데이터를 가져옵니다.
                        const response = await this.$axios.get(tokenURI)
                        const metadata = response.data

                        nftList.push({
                            tokenId: tokenId.toString(),
                            name: metadata.name,
                            description: metadata.description,
                            image: metadata.image,
                            selected: false // 선택 기능 추가
                        })
                    } catch (error) {
                        console.error(`토큰 ID => ${tokenId}  메타데이터를 가져오는 중 오류 발생:`, error)
                    }
                }

                this.nfts = nftList
            } catch (error) {
                console.error('NFT를 가져오는 중 오류 발생:', error)
            } finally {
                this.loading = false
            }
        },

        // async fetchNFTs() {
        //   // 지갑 연결 체크
        //   if (!this.getWalletType || !this.getWalletAddress) {
        //       // connect metamask 안한 경우
        //       this.$noti(this.$q, this.$t('parameter_check_failed_wallet_metamask'))
        //       return
        //   }

        //   this.loading = true
        //   try {
        //       const infuraApiKey = '960c2fe77d1d48f6b4429cd20db6432f' // 여기에 Infura API 키를 넣으세요.
        //       // const provider = new ethers.ethers.providers.JsonRpcProvider(`https://rpc.ankr.com/eth`)
        //       const provider = new ethers.ethers.providers.JsonRpcProvider(`https://sepolia.infura.io/v3/${infuraApiKey}`)
        //       const contractAbi = [
        //           "function tokenURI(uint256 tokenId) view returns (string)",
        //           "function balanceOf(address owner) view returns (uint256)",
        //           "function tokenOfOwnerByIndex(address owner, uint256 index) view returns (uint256)"
        //       ]
        //       const contract = new ethers.Contract(this.contractAddress, contractAbi, provider)

        //       // 사용자가 소유한 NFT의 총 개수
        //       const balance = await contract.balanceOf(this.walletAddress)

        //       // 결과를 저장할 배열 초기화
        //       const nftList = []

        //       // 각 토큰의 ID를 가져와서 메타데이터를 조회
        //       const tokenIdPromises = []
        //       for (let index = 0; index < balance; index++) {
        //           tokenIdPromises.push(contract.tokenOfOwnerByIndex(this.walletAddress, index))
        //       }

        //       const tokenIds = await Promise.all(tokenIdPromises)

        //       const metadataPromises = tokenIds.map(async (tokenId) => {
        //           try {
        //               const tokenURI = await contract.tokenURI(tokenId)
        //               const response = await this.$axios.get(tokenURI)
        //               const metadata = response.data

        //               return {
        //                   tokenId: tokenId.toString(),
        //                   name: metadata.name,
        //                   description: metadata.description,
        //                   image: metadata.image,
        //                   selected: false // 선택 기능 추가
        //               }
        //           } catch (error) {
        //               console.error(`토큰 ID => ${tokenId} 메타데이터를 가져오는 중 오류 발생:`, error)
        //               return null
        //           }
        //       })

        //       const metadataList = await Promise.all(metadataPromises)
        //       this.nfts = metadataList.filter((nft) => nft !== null)
        //   } catch (error) {
        //       console.error('NFT를 가져오는 중 오류 발생:', error)
        //   } finally {
        //       this.loading = false
        //   }
        // },
        async fetchUserNFTs() {
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

              console.log("사용자가 소유한 특정 컨트랙트의 NFT:", response.data);
          } catch (error) {
              console.error("NFT 조회 중 오류 발생:", error);
          }
        },

        // // 80개 이상부터는 너무 많은 요청이 한 번에 이루어질 때 발생할 수 있는 문제 발생
        // async getNftListOfOwner() {
        //   const ownerAddress = this.walletAddress;
        //   const contractAddress = this.contractAddress;

        //   if (!this.$web3) {
        //     throw new Error('web3가 초기화되지 않았습니다.');
        //   }

        //   // 스마트 계약 인스턴스 생성
        //   const contract = new this.$web3.eth.Contract(this.$METAKONGZ_ABI, contractAddress);

        //   try {
        //     // ownerAddress로 받은 모든 Transfer 이벤트 조회
        //     const receivedEvents = await contract.getPastEvents('Transfer', {
        //       filter: { to: ownerAddress },
        //       fromBlock: 0,
        //       toBlock: 'latest'
        //     });

        //     // ownerAddress에서 보낸 모든 Transfer 이벤트 조회
        //     const sentEvents = await contract.getPastEvents('Transfer', {
        //       filter: { from: ownerAddress },
        //       fromBlock: 0,
        //       toBlock: 'latest'
        //     });

        //     // 모든 이벤트를 토큰 ID 별로 정리
        //     const eventMap = new Map();

        //     receivedEvents.forEach(event => {
        //       const tokenId = event.returnValues.tokenId;
        //       const blockNumber = event.blockNumber;

        //       // 받은 이벤트에 해당하는 토큰 ID 저장
        //       if (!eventMap.has(tokenId)) {
        //         eventMap.set(tokenId, { receivedBlock: blockNumber, sentBlock: null });
        //       } else {
        //         eventMap.get(tokenId).receivedBlock = blockNumber;
        //       }
        //     });

        //     sentEvents.forEach(event => {
        //       const tokenId = event.returnValues.tokenId;
        //       const blockNumber = event.blockNumber;

        //       // 보낸 이벤트에 해당하는 토큰 ID 저장
        //       if (!eventMap.has(tokenId)) {
        //         eventMap.set(tokenId, { receivedBlock: null, sentBlock: blockNumber });
        //       } else {
        //         eventMap.get(tokenId).sentBlock = blockNumber;
        //       }
        //     });

        //     // 가장 최근 블록이 receivedBlock인 토큰들만 필터링
        //     const ownedTokenIds = Array.from(eventMap.entries())
        //       .filter(([tokenId, { receivedBlock, sentBlock }]) => 
        //         receivedBlock !== null && (sentBlock === null || receivedBlock > sentBlock))
        //       .map(([tokenId]) => tokenId);

        //     console.log('ownedTokenIds', ownedTokenIds);

        //     // 각 토큰 ID의 메타데이터 URI 가져오기
        //     const tokenURIs = await Promise.all(
        //       ownedTokenIds.map(async (tokenId) => {
        //         const tokenURI = await contract.methods.tokenURI(tokenId).call();
        //         return { tokenId, tokenURI };
        //       })
        //     );

        //     console.log('Token Metadata URIs:', tokenURIs)

        //     this.nftIds = tokenURIs

        //     // // 메타데이터를 URL을 통해 가져오기 (fetch를 사용)
        //     // const metadataList = await Promise.all(
        //     //   tokenURIs.map(async ({ tokenId, tokenURI }) => {
        //     //     const response = await fetch(tokenURI);
        //     //     const metadata = await response.json();
        //     //     return { tokenId, metadata };
        //     //   })
        //     // );

        //     // console.log('Token Metadata:', metadataList);

        //   } catch (error) {
        //     console.error('NFT를 가져오는 중 오류 발생:', error);
        //   }
        // },

        async getNftListOfOwner() {
          const ownerAddress = this.walletAddress
          const contractAddress = this.contractAddress

          if (!this.$web3) {
            throw new Error('web3가 초기화되지 않았습니다.')
          }

          // 스마트 계약 인스턴스 생성
          const contract = new this.$web3.eth.Contract(this.$METAKONGZ_ABI, contractAddress)

          try {
            // ownerAddress로 받은 모든 Transfer 이벤트 조회
            const receivedEvents = await contract.getPastEvents('Transfer', {
              filter: { to: ownerAddress },
              fromBlock: 0,
              toBlock: 'latest'
            })

            // ownerAddress에서 보낸 모든 Transfer 이벤트 조회
            const sentEvents = await contract.getPastEvents('Transfer', {
              filter: { from: ownerAddress },
              fromBlock: 0,
              toBlock: 'latest'
            })

            // 모든 이벤트를 토큰 ID 별로 정리
            const eventMap = new Map()

            receivedEvents.forEach(event => {
              const tokenId = event.returnValues.tokenId
              const blockNumber = event.blockNumber

              // 받은 이벤트에 해당하는 토큰 ID 저장
              if (!eventMap.has(tokenId)) {
                eventMap.set(tokenId, { receivedBlock: blockNumber, sentBlock: null })
              } else {
                eventMap.get(tokenId).receivedBlock = blockNumber
              }
            })

            sentEvents.forEach(event => {
              const tokenId = event.returnValues.tokenId;
              const blockNumber = event.blockNumber;

              // 보낸 이벤트에 해당하는 토큰 ID 저장
              if (!eventMap.has(tokenId)) {
                eventMap.set(tokenId, { receivedBlock: null, sentBlock: blockNumber })
              } else {
                eventMap.get(tokenId).sentBlock = blockNumber
              }
            })

            // 가장 최근 블록이 receivedBlock인 토큰들만 필터링
            const ownedTokenIds = Array.from(eventMap.entries())
              .filter(([tokenId, { receivedBlock, sentBlock }]) =>
                receivedBlock !== null && (sentBlock === null || receivedBlock > sentBlock))
              .map(([tokenId]) => tokenId)

            console.log('ownedTokenIds', ownedTokenIds)

            // 배치 크기 설정
            const batchSize = 40;
            let allTokenURIs = [];

            // 배치로 나누어 처리
            for (let i = 0; i < ownedTokenIds.length; i += batchSize) {
              console.log(i)
              const batchTokenIds = ownedTokenIds.slice(i, i + batchSize)

              // 각 배치의 토큰 ID에 대한 tokenURI 가져오기
              const tokenURIs = await Promise.all(
                batchTokenIds.map(async (tokenId) => {
                  const tokenURI = await contract.methods.tokenURI(tokenId).call()

                  const imageUrl = `https://eth-kongz.by-syl.com/images/${tokenId}.png`
                  const name = `Kongz#${tokenId}`

                  return { tokenId, tokenURI, image: imageUrl, name }
                })
              );

              // 배치별로 결과 추가
              allTokenURIs = [...allTokenURIs, ...tokenURIs]
            }

            console.log('Token Metadata URIs:', allTokenURIs)

            this.nftIds = allTokenURIs

            // 만약 추가적으로 메타데이터를 가져오고 싶다면 다음과 같이 진행
            // const metadataList = await Promise.all(
            //   allTokenURIs.map(async ({ tokenId, tokenURI }) => {
            //     const response = await fetch(tokenURI);
            //     const metadata = await response.json();
            //     return { tokenId, metadata };
            //   })
            // );
            // console.log('Token Metadata:', metadataList);

          } catch (error) {
            console.error('NFT를 가져오는 중 오류 발생:', error)
          }
        },


        selectAllNFTs() {
            this.nfts.forEach(nft => {
                nft.selected = true
            })
        },

        deselectAllNFTs() {
            this.nfts.forEach(nft => {
                nft.selected = false
            })
        },

        async requestAirdrop() {
            const selectedNFTs = this.nfts.filter(nft => nft.selected)
            if (selectedNFTs.length === 0) {
                console.error('선택된 NFT가 없습니다.')
                return
            }

            try {
                // 선택된 NFT들을 사용하여 에어드랍 요청 로직 추가
                console.log('에어드랍 요청을 수행합니다:', selectedNFTs)
                // 실제 에어드랍 로직을 여기에 추가하십시오.
            } catch (error) {
                console.error('에어드랍 요청 중 오류가 발생했습니다:', error)
            }
        }
    }
})
</script>

<style scoped>
:deep(.material-icons) {
    font-family: 'Material Icons' !important;
}
</style>