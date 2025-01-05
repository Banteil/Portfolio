import os
import json
from collections import OrderedDict

def update_json_file(input_path, output_path):
    # 파일명에서 숫자 부분 추출 (예: 101.json -> 101)
    filename = os.path.basename(input_path)
    nft_number = filename.split('.')[0]

    with open(input_path, 'r', encoding='utf-8') as file:
        try:
            data = json.load(file, object_pairs_hook=OrderedDict)
        except json.JSONDecodeError:
            print(f"Error reading JSON in {input_path}")
            return

    # Step 1: image 필드 수정
    if 'image' in data:
        data['image'] = f"https://punkykongz.com/nft/santa/images/{nft_number}.png"

    # # Step 2: royalty 정보를 seller_fee_basis_points로 이동
    # seller_fee = None
    # if 'properties' in data and 'royalty' in data['properties']:
    #     seller_fee = data['properties']['royalty']
    #     del data['properties']['royalty']

    # # Step 3: collection 정보 추가 (최상위 레벨에 추가)
    # if 'collection' not in data:
    #     data['collection'] = {
    #         "name": "Punky Kongz",
    #         "family": "Punky Kongz"
    #     }

    # # Step 4: description 업데이트
    # new_description = (
    #     "Everything we do is driven by our belief: \"To challenge an unfair society.\" "
    #     "We believe in the values of adventurers and dreamers who have shaken the world. "
    #     "Our way of challenging an unfair society is by discovering Punkyvists, who advocate "
    #     "and support the innovative value of PUNKVISM, and by building and expanding a revolutionary "
    #     "community based on their strong network. "
    #     "Through K-NFTs and RWA, we aim to present a differentiated strategy in the global market, "
    #     "offering a new paradigm while expanding our global influence."
    # )
    # if 'description' in data:
    #     data['description'] = new_description

    # # Step 5: Birthday 값을 2024.11.20.로 통일 (문자열)하고 display_type 제거
    # if 'attributes' in data:
    #     for attr in data['attributes']:
    #         if attr.get('trait_type') == 'Birthday':
    #             attr['value'] = "2024.11.20."  # 문자열로 변경
    #             if 'display_type' in attr:
    #                 del attr['display_type']  # display_type 제거

    # # Step 6: properties에서 필요 없는 항목 제거하고 creators만 유지
    # if 'properties' in data:
    #     data['properties'] = {
    #         "creators": data['properties'].get('creators', [])
    #     }

    # # Step 7: attributes 정렬
    # trait_order = [
    #     "Nose", "Glass", "Cap", "Ear", "Neck", 
    #     "Face", "Cloth", "Body", "Wing", "Background"
    # ]
    # if 'attributes' in data:
    #     attributes = data['attributes']

    #     # Birthday와 Rarity는 항상 맨 위에 유지
    #     fixed_attributes = [attr for attr in attributes if attr.get('trait_type') in ["Birthday", "Rarity"]]

    #     # 나머지 항목을 trait_order 순서로 정렬
    #     sorted_attributes = sorted(
    #         [attr for attr in attributes if attr not in fixed_attributes],
    #         key=lambda x: (
    #             trait_order.index(x['trait_type']) if x['trait_type'] in trait_order else len(trait_order)
    #         )
    #     )

    #     # attributes를 다시 합치기
    #     data['attributes'] = fixed_attributes + sorted_attributes

    # # Step 8: symbol 추가 및 위치 조정 (name 아래에 추가)
    # if 'symbol' not in data:
    #     updated_data = OrderedDict()
    #     for key, value in data.items():
    #         updated_data[key] = value
    #         if key == 'name':
    #             updated_data['symbol'] = "PUNKY"
    #     data = updated_data
    # else:
    #     updated_data = data

    # # Step 9: seller_fee_basis_points를 external_url 위에 추가
    # if seller_fee is not None:
    #     final_data = OrderedDict()
    #     for key, value in updated_data.items():
    #         if key == 'external_url':
    #             final_data['seller_fee_basis_points'] = seller_fee
    #         final_data[key] = value
    #     updated_data = final_data

    # 수정된 JSON 데이터를 새로운 파일로 저장
    with open(output_path, 'w', encoding='utf-8') as file:
        json.dump(data, file, indent=4, ensure_ascii=False)
    print(f"Saved updated JSON to {output_path}")


def update_all_json_files(input_directory, output_directory, batch_size=100):
    # 수정된 파일을 저장할 폴더 생성 (없으면 생성)
    if not os.path.exists(output_directory):
        os.makedirs(output_directory)

    # 입력 폴더에서 모든 .json 파일 찾기
    json_files = [f for f in os.listdir(input_directory) if f.endswith('.json')]
    
    # 파일들을 batch_size 단위로 처리
    for i in range(0, len(json_files), batch_size):
        batch_files = json_files[i:i + batch_size]
        for filename in batch_files:
            input_path = os.path.join(input_directory, filename)
            output_path = os.path.join(output_directory, filename)
            update_json_file(input_path, output_path)

        # 배치 완료 후 메시지 출력
        print(f"Processed batch {i // batch_size + 1} / {len(json_files) // batch_size + 1}")


if __name__ == "__main__":
    # 현재 스크립트가 있는 경로를 기준으로 폴더 설정
    current_dir = os.path.dirname(os.path.abspath(__file__))
    input_directory = os.path.join(current_dir, 'NFT_METADATA')
    output_directory = os.path.join(current_dir, 'NFT_JSON_REPLACE')

    # 입력 폴더가 존재하는지 확인 후 실행
    if os.path.exists(input_directory):
        update_all_json_files(input_directory, output_directory, batch_size=100)
    else:
        print(f"Input directory not found: {input_directory}")
