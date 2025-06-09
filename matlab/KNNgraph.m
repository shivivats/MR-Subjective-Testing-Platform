function minDistance = KNNgraph(vertices, theta)

fprintf("Calculating %d nearest neighbours from %d vertices \n" , theta+1, size(vertices, 1));

% Compute k nearest neighbors
[~, dis] = knnsearch(vertices, vertices, 'K', theta+1);
if theta+1 > length(dis)
    dis_avg = mean(dis(:,1:length(dis)), 2);
else
    dis_avg = mean(dis(:,1:theta+1), 2);
end
dis_avg = sort(dis_avg);
% Plot the k-NNG

x = 1:1:size(dis_avg,1);
y = dis_avg;

%x = dis_avg;
%y = 1:1:size(dis_avg,1);


% Compute the slope of each point on the curved line
for i = 1:(length(x)-1)
    slope(i) = (y(i+1)-y(i))/(x(i+1)-x(i));
end

[~,index] = max(slope);

if length(y) <= 50
    minDistance = y(1);
else
    if index <= 50
        minDistance = y(index);
    else
        minDistance = y(index-50);
    end    
end

%minDistance = y(index);

fprintf("Index of max slope from KNNSearch is %d \n" , index);
fprintf("Epsilon from KNNSearch is %f \n" , minDistance);

x_slope = 2:1:size(dis_avg,1);
y_slope = slope;

%x_slope = slope;
%y_slope =  2:1:size(dis_avg,1);

figure
hold on;
plot(x,y,'r')
%plot(y,x,'r');
hold on;
scatter(x_slope,y_slope,'green')
%scatter(y_slope,x_slope,'green')
hold off;
title(sprintf('min Distance is %.5f;',minDistance));
xlabel('X: k-Distance');
ylabel(sprintf('Y: K= %d',theta));
%xlabel(sprintf('Y: K= %d',theta));
%ylabel('X: k-Distance');
grid on;


end