import os
import json
import pandas as pd
from collections import defaultdict

# 엑셀 파일 경로 및 시트 이름 설정
excel_file_path = 'table.xlsx'
sheet_name = 'Scarcity Table'

PARTS_DIR = "Parts"
NFT_JSON_DIR = "NFT_JSON"
LOG_FILE_PATH = "nft_trait_analysis_log.txt"
INVALID_JSON_DIR = "FIND_INVALID_JSON"  # 잘못된 JSON 저장 폴더

os.makedirs(INVALID_JSON_DIR, exist_ok=True)

part_columns = {
    "Background": (5, 6, 7),  # 5: 파츠명, 6: 확률, 7: 개체수
    "Clothes": (35, 36, 37),
    "Eyes": (23, 24, 25),
    "Head": (11, 12, 13),
    "Mouth": (29, 30, 31),
    "Skin": (17, 18, 19),
    "Tail": (41, 42, 43),
}

# 엑셀 데이터 로드
df = pd.read_excel(excel_file_path, sheet_name=sheet_name, header=None)

# 파츠 제한 및 확률 설정 함수
def load_part_limits(df):
    part_limits = {}
    part_probabilities = {}

    # 엑셀에서 파츠 데이터 로드
    for layer, (num_col, name_col, prob_col, count_col) in part_columns.items():
        part_limits[layer] = {}
        part_probabilities[layer] = {}

        for i in range(df.shape[0]):
            part_name = df.iloc[i, name_col]
            max_count = df.iloc[i, count_col]
            probability = df.iloc[i, prob_col]

            if pd.notna(part_name) and pd.notna(max_count) and pd.notna(probability):
                try:
                    part_limits[layer][part_name] = int(max_count)
                    part_probabilities[layer][part_name] = float(probability)
                except ValueError:
                    continue

    return part_limits, part_probabilities

# 파츠별 현재 총 생성 개수 및 남은 생성 개수 계산
def calculate_part_creation_counts(part_limits, nft_json_dir):
    # 파츠별 생성 개수를 기록할 딕셔너리
    part_creation_counts = {layer: defaultdict(int) for layer in part_limits}

    # JSON 파일을 읽어 생성된 파츠 개수 계산
    for file_name in os.listdir(nft_json_dir):
        if file_name.endswith(".json"):
            file_path = os.path.join(nft_json_dir, file_name)
            try:
                with open(file_path, "r") as json_file:
                    data = json.load(json_file)
                    attributes = data.get("attributes", [])
                    for trait in attributes:
                        trait_type = trait.get("trait_type")
                        trait_value = trait.get("value")
                        if trait_type and trait_value and trait_type in part_limits:
                            part_creation_counts[trait_type][trait_value] += 1
            except Exception as e:
                print(f"Error reading {file_name}: {e}")

    # 각 파츠의 남은 생성 개수 계산
    remaining_counts = {layer: {} for layer in part_limits}
    for layer, parts in part_limits.items():
        for part_name, max_count in parts.items():
            created_count = part_creation_counts[layer].get(part_name, 0)
            remaining_count = max(max_count - created_count, 0)
            if remaining_count > 0:  # 남은 개수가 0보다 큰 경우만 포함
                remaining_counts[layer][part_name] = remaining_count

    return part_creation_counts, remaining_counts

# 정의되지 않은 파츠(엑셀에 없는 파츠)를 찾고 수정하는 함수
def find_and_fix_invalid_traits(part_limits, nft_json_dir, invalid_json_dir):
    invalid_traits = []  # 잘못된 트레이트를 기록할 리스트

    for file_name in os.listdir(nft_json_dir):
        if file_name.endswith(".json"):
            file_path = os.path.join(nft_json_dir, file_name)
            try:
                with open(file_path, "r") as json_file:
                    data = json.load(json_file)
                    attributes = data.get("attributes", [])
                    nft_number = data.get("nft_number", file_name)  # NFT 번호 가져오기
                    fixed = False  # 수정 여부 확인

                    for trait in attributes:
                        trait_type = trait.get("trait_type")
                        trait_value = trait.get("value")
                        if trait_type in part_limits and trait_value not in part_limits[trait_type]:
                            invalid_traits.append({
                                "nft_number": nft_number,
                                "layer": trait_type,
                                "invalid_value": trait_value,
                                "file_name": file_name
                            })
                            # 잘못된 파츠를 'none'으로 수정
                            trait["value"] = "none"
                            fixed = True

                    # 수정된 JSON을 FIND_INVALID_JSON 폴더에 저장
                    if fixed:
                        fixed_file_path = os.path.join(invalid_json_dir, file_name)
                        with open(fixed_file_path, "w") as fixed_file:
                            json.dump(data, fixed_file, indent=4, ensure_ascii=False)
            except Exception as e:
                print(f"Error reading {file_name}: {e}")

    return invalid_traits

# 기존 로그 함수 확장
def analyze_traits_and_log(part_limits, part_creation_counts, remaining_counts, log_file_path, invalid_traits):
    with open(log_file_path, "w") as log_file:
        print("NFT Trait Analysis :")
        log_file.write("NFT Trait Analysis :\n")
        
        # 각 파츠별 생성된 개수 출력
        for layer, parts in part_limits.items():
            print(f"\n{layer}:")
            log_file.write(f"\n{layer}:\n")
            for part_name, max_count in parts.items():
                created_count = part_creation_counts[layer].get(part_name, 0)
                line = f"  {part_name}: {created_count}개"
                print(line)
                log_file.write(line + "\n")
        
        # 각 파츠별 남은 생성 개수 출력
        print("\nRemaining parts to generate (in JSON format):")
        log_file.write("\nRemaining parts to generate (in JSON format):\n")
        
        remaining_json = {}
        for layer, parts in remaining_counts.items():
            remaining_json[layer] = {part_name: remaining_counts[layer][part_name] for part_name in parts}
            layer_json = json.dumps(remaining_json[layer], ensure_ascii=False, indent=4)
            layer_json = f"\"{layer}\": {layer_json},"
            print(layer_json)
            log_file.write(layer_json + "\n")

        # 레이어별 총 생성 개수 및 남은 개수 출력
        print("\nTotal and remaining counts by layer:")
        log_file.write("\nTotal and remaining counts by layer:\n")
        for layer, parts in part_limits.items():
            total_count = sum(parts.values())
            remaining_count = sum(remaining_counts[layer].values())
            line = f"{layer}: Total Created = {total_count}, Remaining = {remaining_count}"
            print(line)
            log_file.write(line + "\n")

        # 잘못된 트레이트 출력
        print("\nInvalid Traits Analysis:")
        log_file.write("\nInvalid Traits Analysis:\n")
        if not invalid_traits:
            print("No invalid traits found in any NFT.")
            log_file.write("No invalid traits found in any NFT.\n")
        else:
            for trait in invalid_traits:
                line = (f"NFT #{trait['nft_number']} in file '{trait['file_name']}' "
                        f"has invalid value '{trait['invalid_value']}' for layer '{trait['layer']}'")
                print(line)
                log_file.write(line + "\n")

    print(f"\nAnalysis complete. Log saved to {log_file_path}")

# 엑셀에서 파츠 제한 및 확률 로드
part_limits, part_probabilities = load_part_limits(df)

# 파츠별 생성 개수 및 남은 개수 계산
part_creation_counts, remaining_counts = calculate_part_creation_counts(part_limits, NFT_JSON_DIR)

# 정의되지 않은 파츠 찾고 수정
invalid_traits = find_and_fix_invalid_traits(part_limits, NFT_JSON_DIR, INVALID_JSON_DIR)

# 결과 분석 및 로그 저장
analyze_traits_and_log(part_limits, part_creation_counts, remaining_counts, LOG_FILE_PATH, invalid_traits)
