import os
import json
from datetime import datetime, timedelta

def generate_metadata(nft_number):
    metadata = {
        "image": f"https://punkykongz.com/nft/images/{nft_number}.png",
        "description": (
            "Everything we do is driven by our belief: \"To challenge an unfair society.\" "
            "We believe in the values of adventurers and dreamers who have shaken the world. "
            "Our way of challenging an unfair society is by discovering Punkyvists, who advocate "
            "and support the innovative value of PUNKVISM, and by building and expanding a revolutionary "
            "community based on their strong network. Through K-NFTs and RWA, we aim to present a differentiated "
            "strategy in the global market, offering a new paradigm while expanding our global influence."
        ),
        "name": f"Punky Kongz #{nft_number}",
        "symbol": "PUNKY",
        "seller_fee_basis_points": 500,
        "external_url": "https://punkykongz.com",
        "attributes": [
            {
                "display_type": "date",
                "trait_type": "Birthday",
                "value": int((datetime(2024, 1, 1) + timedelta(days=nft_number)).timestamp())
            },
            {
                "trait_type": "Rarity",
                "value": "Legendary"
            }
        ],
        "properties": {
            "creators": [
                {
                    "address": "8UwXdUWyN3FRtsMtqK3Xa8sa1DFdrE1c5mu11Z8z1maQ",
                    "share": 100
                }
            ]
        },
        "collection": {
            "name": "Punky Kongz",
            "family": "Punky Kongz"
        }
    }
    return metadata

def save_metadata(metadata, nft_number, output_directory):
    filename = os.path.join(output_directory, f"{nft_number}.json")
    with open(filename, 'w', encoding='utf-8') as file:
        json.dump(metadata, file, indent=4, ensure_ascii=False)
    print(f"Generated {filename}")

def generate_nft_metadata_files(output_directory, total_files=100):
    if not os.path.exists(output_directory):
        os.makedirs(output_directory)

    for nft_number in range(1, total_files + 1):
        metadata = generate_metadata(nft_number)
        save_metadata(metadata, nft_number, output_directory)

if __name__ == "__main__":
    # 현재 스크립트가 있는 경로를 기준으로 폴더 설정
    current_dir = os.path.dirname(os.path.abspath(__file__))
    output_directory = os.path.join(current_dir, 'NFT_METADATA')

    # 메타데이터 파일 생성
    generate_nft_metadata_files(output_directory, total_files=100)
