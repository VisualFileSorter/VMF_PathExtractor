# VMF Path Extractor

Pass in the path to your VMF and a output txt file:  
Path_Extract.exe "C:\Users\Me\Documents\paths.vmf" "C:\Users\Me\Documents\paths.txt"

Program will extract all path_track paths (except branches) in the below format:  

path_0   | -832     | 64       | 16      
path_1   | -1008    | -64      | 16      
path_2   | -1152    | -464     | 16      
path_3   | -624     | -464     | 16      
path_4   | -352     | -56      | 16      
END PATH


alt_path_track_0 | 448      | 464      | -8      
alt_path_track_1 | 16       | 256      | -8      
alt_path_track_2 | 440      | 208      | -8      
END PATH


non_closed_0 | -176     | -336     | -8      
non_closed_1 | 112      | -88      | -8.00006
non_closed_2 | 440      | -32      | -8      
END PATH


branch_0 | -416     | 200      | -8.00006
branch_1 | -600     | 488      | -8      
END PATH


branch_2 | -304     | 496      | -8.00006
END PATH
