using IDriveView.AddWindows;
using System.Windows;

namespace IDriveView.CreateReqest
{
    class BucketRequestAdvanced
    {
        private MainWindow mainWindow;
        public BucketRequestAdvanced()
        {
            mainWindow = Application.Current.MainWindow as MainWindow;
        }

        // --- Получить список Buckets ---
        public async void ListBucketsAdvancedAsync(object sender, RoutedEventArgs e)
        {
            //var result = await new BucketRequest().GetListBucketsAsync();
            //foreach (var bucket in result)
            //{
            //    Output.WriteLine($"Bucket: {bucket.BucketName}");
            //}
        }
        // --- Получить первый bucket (имя) ---
        public async Task<string> GetFirstBucketAsync()
        {
            var result = await new BucketRequest().GetListBucketsAsync();
            if (result.listBuckets == null)
            {
                await DialogWindows.InformationWindow(result.Message);
                return null;
            }
            return result.listBuckets[0].BucketName;
        }
        // --- Создать Bucket ---
        public async void CreateBucketAdvancedAsync(object sender, RoutedEventArgs e)
        {
            await new BucketRequest().CreateBucketAsync("newBucket");
        }
        // --- Удалить Bucket ---
        public async void DeleteBucketAdvancedAsync(object sender, RoutedEventArgs e)
        {
            await new BucketRequest().DeleteBucketAsync("newBucket");
        }
        // --- Сделать bucket публичным ---
        public async void PrincipalBucketAdvanced(object sender, RoutedEventArgs e)
        {
            await new BucketRequest().PrincipalBucketAsync("newBucket");
        }
        // --- Сделать bucket привантым ---
        public async void PrincipalDeleteBucketAdvanced(object sender, RoutedEventArgs e)
        {
            await new BucketRequest().PrincipalDeleteBucket("newBucket");
        }
        // --- Проверить текущую политику bucket ---
        public async void PrincipalGetBuckeAdvanced(object sender, RoutedEventArgs e)
        {
            await new BucketRequest().PrincipalGetBucket("newBucket");
        }
    }
}
