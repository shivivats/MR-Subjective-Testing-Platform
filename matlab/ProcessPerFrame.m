% Read a file containing vertices of PC frames
% Read the file of weights of PC frames

% If the sum of weights is zero, dont proceed

% theta (minpts) is calculated using point size (0.0012)
% epsilon (search distance) is calculated using the KNN graph

% The DBScan in matlab returns a list of indices of all observation clusters and corePts which is a 0/1 Vector that contains the core pts idenfitied by DBScan.

clear;
close all;

baseDir = "D:\PointCloudsSaved_angle2_time80\Regular\Processed_Sum\";
ptSize = 0.0012;

% Vertices: PointCloudSaved_BlueSpin_frame245_vertices.txt
% Weights: PointCloudSaved_BlueSpin_frame233.txt

%pcNames = ["BlueSpin", "ReadyForWinter", "CasualSquat", "FlowerDance"];
pcNames = [ "ReadyForWinter", "CasualSquat", "FlowerDance"];
%pcNames = ["BlueSpin"];

for pcname = pcNames
    for frameIdx = 14:1:249
        % pcname = "BlueSpin";
        % frameIdx = 121;
        
        verticeFile = baseDir + "NoDbScan\" + "PointCloudSaved_" + pcname + "_frame" + frameIdx + "_vertices.txt";
        weightFile = baseDir + "NoDbScan\"  + "PointCloudSaved_" + pcname + "_frame" + frameIdx + ".txt";
        
        if isfile(verticeFile) 
            if isfile(weightFile)

                fprintf("\n\n Reading data for %s and frame %d  from files %s and %s \n" , pcname, frameIdx, verticeFile, weightFile);
                
                vertices = readmatrix(verticeFile);
                weights = readmatrix(weightFile);

                nonZeroIndices = find(weights ~= 0); % indices where weights are not zero
                nonZeroVertices = vertices(nonZeroIndices, :);               

                % alpha = ptSize
                % theta = minPts
                % epsilon = distance
                
                % theta = 2^7 / (1 + 20*alpha);
                %theta = round(2^7 / (1 + 20*ptSize));
                theta = 100;

                kD = pdist2(nonZeroVertices, nonZeroVertices, 'euclidean', 'Smallest', theta);

                plot(sort(kD(end, :)));
                title("k-distance graph");
                xlabel(sprintf("Points sorted with %d nearest distances, %d total pts.", theta, length(nonZeroVertices)));
                ylabel(sprintf("%dth nearest distances", theta));
                grid

                
                %epsilon = KNNgraph(nonZeroVertices, theta);
                %fprintf("Epsilon from KNNgraph is %f \n" , epsilon);

                minNumPts = 5; % 2*dim-1 = 2*3-1 = 5
                maxNumPts = theta; % minPts are theta 
                epsilon = clusterDBSCAN.estimateEpsilon(nonZeroVertices, theta+1, theta+1);
                fprintf("Epsilon from clusterDBSCAN.estimateEpsilon is %f \n" , epsilon);
                
                fprintf("Starting DBScan for frame %d \n", frameIdx);
                
                [idx, corePts] = dbscan(nonZeroVertices, epsilon, theta);
                
                fprintf("Clusters: %d \n" , max(idx))
                fprintf("PointSize: %f & epsilon is %f\n" , ptSize, epsilon)
                fprintf("MinPoints Number is: %f & Points Number based on the radius is %f\n" , theta, round(4/3*pi*(epsilon*500)^3));
                
                % corePts is a 0/1 vector that contains the indices of core pts identified by dbscan

                nonCorePts = corePts ~= 1;

                xNonCoreVerts = nonZeroVertices(nonCorePts,1);
                yNonCoreVerts = nonZeroVertices(nonCorePts,2);
                zNonCoreVerts = nonZeroVertices(nonCorePts,3);

                xCoreVerts = nonZeroVertices(corePts, 1);
                yCoreVerts = nonZeroVertices(corePts, 2);
                zCoreVerts = nonZeroVertices(corePts, 3);

                plot3(xNonCoreVerts, yNonCoreVerts, zNonCoreVerts,'or'); % non core vertices are red
                hold on
                plot3(xCoreVerts, yCoreVerts, zCoreVerts,'oy'); % core vertices are yellow
                hold off

                fprintf("\nTheta: %d; Total Pts: %d; Core Pts: %d; Non-Core Pts: %d \n",theta, length(nonZeroVertices), length(find(corePts==1)), length(find(nonCorePts==1)));

                
                index_new = corePts ~= 1;


                
                weights_old = weights;
             
                weights(nonZeroIndices(index_new)) = 0; % set weights to zero where DBScan does NOT identify a core point
                
                weightWriteFilename = baseDir + "DbScan\" + "PointCloudSaved_" + pcname + "_frame" + frameIdx + "_dbscan.txt";
                writematrix(weights, weightWriteFilename);

            end
        end
    end
end

