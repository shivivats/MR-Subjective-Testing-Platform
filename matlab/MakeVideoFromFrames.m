% For each point cloud
%   For each orientation
%      Read the frame's generated heatmap image
%      Add the frame to an image array
%      Make video from the image array
%      Name the video appropriately


clear;
close all;

%baseDir = "D:\PointCloudsSaved_angle2_time80\Regular\Screenshots_Sum\";
baseDir = "D:\PointCloudsSaved_angle3_time120\Regular\Screenshots_Sum\";

pcNames = ["BlueSpin", "ReadyForWinter", "FlowerDance","CasualSquat"];

orientations = ["front", "back", "left", "right"];
%dbscan = ["DbScan\", "NoDbScan\"];
dbscan = ["DbScan\"];

videoFps = 25;

% Frame: FlowerDance_249_back.png

for dbs = dbscan
    for pcname = pcNames
        for orient = orientations
            fprintf("\n Reading data for %s and orientation %s from directory %s\n" , pcname, orient, baseDir + dbs);
            videoWriter = VideoWriter(baseDir + dbs + pcname + "_" + orient + "_fps" + videoFps + ".mp4", 'MPEG-4');
            videoWriter.FrameRate = videoFps;
            open(videoWriter);
            
            for frameIdx = 14:1:249
                frameFile = baseDir + dbs + pcname + "_" + orient + "_" + frameIdx + ".png";
                frameImage = imread(frameFile);
                writeVideo(videoWriter, frameImage);
            end

            close(videoWriter);

            fprintf("Made video for %s and orientation %s from directory %s and stored it at %s \n" , pcname, orient, baseDir + dbs, baseDir + dbs + pcname + "_" + orient + ".avi");
        end
    end
end