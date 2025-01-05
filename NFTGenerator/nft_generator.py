import os
import json
import random
import pandas as pd
from datetime import datetime, timezone
from PIL import Image
import subprocess

# 경로 및 기본 설정
excel_file_path = 'table.xlsx'
sheet_name = 'Scarcity Table'
PARTS_DIR = "Parts"
LAYERS = ["Background", "Tail", "Skin", "Eyes", "Head", "Clothes", "Mouth"]
NFT_IMAGE_DIR = "NFT_IMAGE"
NFT_JSON_DIR = "NFT_JSON"
LOGS_DIR = "Logs"
LOG_FILE_PATH = os.path.join(LOGS_DIR, "generating_log.txt")

os.makedirs(NFT_IMAGE_DIR, exist_ok=True)
os.makedirs(NFT_JSON_DIR, exist_ok=True)
os.makedirs(LOGS_DIR, exist_ok=True)

description = "Everything we do is driven by our belief: \"To challenge an unfair society.\" We believe in the values of adventurers and dreamers who have shaken the world. Our way of challenging an unfair society is by discovering Punkyvists, who advocate and support the innovative value of PUNKVISM, and by building and expanding a revolutionary community based on their strong network. Through K-NFTs and RWA, we aim to present a differentiated strategy in the global market, offering a new paradigm while expanding our global influence."
creator_wallet_address = "8UwXdUWyN3FRtsMtqK3Xa8sa1DFdrE1c5mu11Z8z1maQ"
external_url = "https://punkykongz.com"
royalties_fee = 500
collection_name = "Punky Kongz"
symbol = "PUNKY"
target_birthday = "2024.12.26."
start_number = 1

# 파츠 정보 (엑셀 컬럼 정의)
part_columns = {
    "Background": (5, 6, 7),  # 5: 파츠명, 6: 확률, 7: 개체수
    "Clothes": (35, 36, 37),
    "Eyes": (23, 24, 25),
    "Head": (11, 12, 13),
    "Mouth": (29, 30, 31),
    "Skin": (17, 18, 19),
    "Tail": (41, 42, 43),
}

def append_log(message):
    with open(LOG_FILE_PATH, "a") as log_file:
        log_file.write(f"{message}\n")

# 엑셀 데이터 로드 및 파츠 정보 초기화
def load_part_limits(df):
    part_limits = {}
    part_probabilities = {}
    for layer, (name_col, prob_col, count_col) in part_columns.items():
        part_limits[layer] = {}
        part_probabilities[layer] = {}
        for i in range(df.shape[0]):
            part_name = df.iloc[i, name_col]
            probability = df.iloc[i, prob_col]
            max_count = df.iloc[i, count_col]

            if pd.notna(part_name) and pd.notna(max_count) and pd.notna(probability):
                try:
                    part_limits[layer][part_name] = int(max_count)
                    part_probabilities[layer][part_name] = float(probability)
                except ValueError:
                    continue
    # 로그 저장
    append_log("\n[Excel 데이터 로드 결과]")
    for layer, limits in part_limits.items():
        append_log(f"Layer: {layer}")
        for part, max_count in limits.items():
            append_log(f"  - Part: {part}, Max Count: {max_count}, Probability: {part_probabilities[layer][part]}")
    return part_limits

df = pd.read_excel(excel_file_path, sheet_name=sheet_name, header=None)
total_count = int(df.iloc[0, 1]) if pd.notna(df.iloc[0, 1]) else 0
checkpoints_df = df.iloc[4:, [0, 1]].dropna()
checkpoints = checkpoints_df.iloc[:, 1].astype(int).tolist()
part_limits = load_part_limits(df)

# 중복 조합 방지
def load_existing_combinations(nft_dir):
    existing_combinations = set()
    if not os.path.exists(nft_dir):
        return existing_combinations
    for file in os.listdir(nft_dir):
        if file.endswith('.json'):
            with open(os.path.join(nft_dir, file), 'r') as f:
                metadata = json.load(f)
                combination_str = ",".join(
                    [attr["value"] for attr in metadata["attributes"] if attr["trait_type"] != "Birthday"]
                )
                existing_combinations.add(combination_str)
    return existing_combinations

# 로그 저장
def save_checkpoint_log(nft_number, traits, cumulative_counts, part_limits):
    log_file_path = os.path.join(LOGS_DIR, f"nft_{nft_number}_log.json")
    log_data = {
        "nft_number": nft_number,
        "traits": traits,
        "cumulative_counts": {
            layer: {
                part: {
                    "created": cumulative_counts[layer][part],
                    "target": part_limits[layer][part]
                }
                for part in cumulative_counts[layer]
            }
            for layer in cumulative_counts
        }
    }
    with open(log_file_path, 'w') as log_file:
        json.dump(log_data, log_file, indent=4)
    print(f"Log saved for NFT #{nft_number}")

# 이미지 저장 및 최적화 함수
def save_optimized_image(image, nft_number):
    nft_image_path = os.path.join(NFT_IMAGE_DIR, f"{nft_number}.png")
    if image.mode == 'RGBA':
        image = image.convert('RGB')
    image.save(nft_image_path, format="PNG", optimize=True)
    optimize_with_pngquant(nft_image_path)

# pngquant를 사용한 최적화 함수
def optimize_with_pngquant(image_path):
    pngquant_path = "pngquant" if os.name != "nt" else "pngquant.exe"
    if not os.path.exists(pngquant_path):
        print(f"Error: {pngquant_path} not found")
        return
    try:
        subprocess.run(
            [pngquant_path, "--force", "--quality=50-90", "--ext", ".png", image_path],
            check=True
        )
        print(f"Optimized with pngquant: {image_path}")
    except subprocess.CalledProcessError as e:
        print(f"Error optimizing with pngquant: {e}")

# 메타데이터 생성
def generate_metadata(nft_number, traits):
    # LAYERS를 역순으로 정렬
    reversed_layers = LAYERS[::-1]
    
    # traits를 reversed_layers 순서로 정렬하고 "none" 값을 "None"으로 변환
    sorted_traits = [
        {"trait_type": layer, "value": "None" if traits[layer] == "none" else traits[layer]}
        for layer in reversed_layers if layer in traits
    ]

    return {
        "image": f"https://punkykongz.com/nft/images/{nft_number}.png",
        "description": description,
        "name": f"Test Fitri #{nft_number}",
        "symbol": symbol,
        "seller_fee_basis_points": royalties_fee,
        "external_url": external_url,
        "attributes": [{"trait_type": "Birthday", "value": target_birthday}] +
                      [{"trait_type": "Rarity", "value": "Common"}] +
                      sorted_traits,
        "properties": {"creators": [{"address": creator_wallet_address, "share": 100}]},
        "collection": {"name": collection_name, "family": collection_name}
    }


# NFT 생성
# 확률 기반 NFT 생성
def generate_nft_image_and_metadata(total_count, part_limits, checkpoints):
    generated_combinations = load_existing_combinations(NFT_JSON_DIR)
    checkpoints.append(total_count)  # 마지막 체크포인트 추가
    cumulative_counts = {layer: {part: 0 for part in part_limits[layer]} for layer in part_limits}

    nft_number = start_number  # NFT 번호는 항상 start_number에서 시작
    start = 1  # 체크포인트 시작 지점

    for checkpoint in checkpoints:
        # 현재 체크포인트에서 생성해야 할 개체 수
        count_in_checkpoint = checkpoint - start + 1
        print(f"\nGenerating NFTs from {nft_number} (start={start}) to {nft_number + count_in_checkpoint - 1}...")

        for _ in range(count_in_checkpoint):
            successful = False
            max_attempts = 1000  # 최대 시도 횟수
            attempt_count = 0
            final_traits = None  # 최종 확정된 트레이트
            final_combination_str = None  # 최종 조합 문자열

            while not successful and attempt_count < max_attempts:
                traits = {}
                combination = []
                temp_counts = {layer: cumulative_counts[layer].copy() for layer in cumulative_counts}  # 임시 카운팅

                # 파츠 선택
                for layer in LAYERS:
                    parts = list(part_limits[layer].keys())
                    available_parts = [
                        part for part in parts if temp_counts[layer][part] < part_limits[layer][part]
                    ]

                    if not available_parts:
                        traits[layer] = "none"
                        combination.append("none")
                        continue

                    # 확률 계산
                    total_remaining = sum([part_limits[layer][part] - temp_counts[layer][part] for part in available_parts])
                    probabilities = [
                        (part_limits[layer][part] - temp_counts[layer][part]) / total_remaining
                        for part in available_parts
                    ]

                    # 확률 로그 출력
                    append_log(f"\n[Layer: {layer}]")
                    for part, prob in zip(available_parts, probabilities):
                        append_log(f"  - Part: {part}, Probability: {prob:.6f}, Remaining: {part_limits[layer][part] - temp_counts[layer][part]}")

                    # 확률 기반 선택
                    selected_part = random.choices(available_parts, weights=probabilities, k=1)[0]
                    traits[layer] = selected_part
                    combination.append(selected_part)
                    temp_counts[layer][selected_part] += 1  # 임시로 증가값 적용

                combination_str = ",".join(combination)
                if combination_str not in generated_combinations:
                    # 중복이 아니면 성공
                    generated_combinations.add(combination_str)
                    successful = True
                    final_traits = traits
                    final_combination_str = combination_str
                    cumulative_counts = temp_counts  # 임시 값을 최종 반영
                else:
                    # 중복 조합이면 폐기하고 다시 시도
                    attempt_count += 1

            # 시도 횟수 초과 시 강제 생성
            if not successful:
                print(f"Warning: Could not generate unique combination after {max_attempts} attempts for NFT #{nft_number}. Using repeated combination.")
                final_traits = traits
                final_combination_str = combination_str
                generated_combinations.add(final_combination_str)  # 중복으로 추가
                cumulative_counts = temp_counts  # 임시 값 반영

            # 이미지 생성 및 저장
            layers = []
            for layer in LAYERS:
                part_name = final_traits[layer]
                if part_name != "none":
                    part_image_path = os.path.join(PARTS_DIR, layer, f"{part_name}.png")
                    if os.path.exists(part_image_path):
                        image = Image.open(part_image_path).convert("RGBA")
                        layers.append(image)

            if layers:
                base_image = layers[0]
                for layer_img in layers[1:]:
                    base_image.paste(layer_img, (0, 0), layer_img)
                save_optimized_image(base_image, nft_number)

                metadata = generate_metadata(nft_number, final_traits)
                with open(os.path.join(NFT_JSON_DIR, f"{nft_number}.json"), 'w') as f:
                    json.dump(metadata, f, indent=4)

            # 로그 저장
            save_checkpoint_log(nft_number, final_traits, cumulative_counts, part_limits)
            print(f"Generated NFT #{nft_number} with traits: {final_traits}")

            nft_number += 1  # NFT 번호 증가

        start = checkpoint + 1  # 다음 체크포인트 시작 지점

    # 생성 완료 후 요약 출력
    print("\nNFT Generation Summary")
    print("======================")
    print(f"Generated NFTs from #{start_number} to #{nft_number - 1}")
    total_created = nft_number - start_number
    print(f"Total NFTs generated: {total_created}")
    print("\nParts Generated per Layer:")
    for layer, parts in cumulative_counts.items():
        print(f"- {layer}:")
        for part, count in parts.items():
            print(f"  {part}: {count}")

# 실행
generate_nft_image_and_metadata(total_count, part_limits, checkpoints)
