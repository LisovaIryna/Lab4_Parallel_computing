using System.Diagnostics;

namespace Lab4;

public partial class MainPage : ContentPage
{
    public int[] result;
    public Stopwatch stopWatch = new Stopwatch();

    public MainPage()
	{
		InitializeComponent();
    }

    // create array from 1 to selected number
    private int[] CountAllElements(int count)
    {
        var countElements = new int[count];
        for (int i = 0; i < count; i++)
        {
            countElements[i] = i + 1;
        }

        return countElements;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        stopWatch.Reset();
        stopWatch.Start();

        ResultPermutation.Text= string.Empty;
        ResultAll.Text = string.Empty;
        TimeExecuteProgram.Text = string.Empty;

        if (CountElements.SelectedItem == null)
        {
            DisplayAlert("Помилка", "Не обрано максимальне число в перестановці.", "OK");
            return;
        }

        var countElements = CountAllElements((int)CountElements.SelectedItem);

        result = new int[countElements.Length];

        // the first thread declaration
        Thread calculatepermutations = new(() => Permute(countElements))
        {
            Name = "Обчислення та виведення перестановок"
        };
        // start the first thread
        calculatepermutations.Start();
    }

    private void Permute(int[] nums)
    {
        var list = new List<IList<int>>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ResultPermutation.Text += $"Всі перестановки з 1 до {nums.Length}:\n\n";
        });

        int countPermutations = CountPermutationFactorial(nums.Length);
        IList<IList<int>> matrixPermutations = DoPermute(nums, 0, nums.Length - 1, list, countPermutations);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            for (int i = 0; i < countPermutations; i++)
            {
                ResultPermutation.Text += $"[{string.Join(',', matrixPermutations[i])}]";
                ResultPermutation.Text += "\n";
            }
        });
    }

    public IList<IList<int>> DoPermute(int[] nums, int start, int end, IList<IList<int>> list, int countPermutations)
    {
        if (start == end)
        {
            list.Add(new List<int>(nums));
            // the second thread declaration
            Thread multiplyPermutations = new(() => MultiplyPermutations(list[list.Count - 2], list[list.Count - 1], list.Count, nums))
            {
                Name = "Множення перестановок та виведення результату"
            };
            if (list.Count >= 2)
            {
                // start the second thread
                multiplyPermutations.Start();
                multiplyPermutations.Join();
            }
        }
        else
        {
            for (var i = start; i <= end; i++)
            {
                Swap(ref nums[start], ref nums[i]);
                DoPermute(nums, start + 1, end, list, countPermutations);
                Swap(ref nums[start], ref nums[i]);
            }
        }

        return list;
    }

    // multiply two permitations which are parameters
    private void MultiplyPermutations(IList<int> firstMatrixPermutation, IList<int> secondMatrixPermutation, int countMatrixPermutations,  int[] countElements)
    {
        for (int j = 0; j < countElements.Length; j++)
        {
            if (countMatrixPermutations == 2)
            {
                if (firstMatrixPermutation[j] == j + 1)
                    result[j] = secondMatrixPermutation[j];
            }
            else
            {
                int l = 0;
                while (result[j] != l + 1)
                {
                    l++;
                }
                result[j] = secondMatrixPermutation[l];
            }
        }

        if (countMatrixPermutations == CountPermutationFactorial(countElements.Length))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ResultAll.Text += $"Результат множення {CountPermutationFactorial(countElements.Length)} перестановок:\n\n";
                ResultAll.Text += "[";
                for (int i = 0; i < result.Length; i++)
                {
                    if (i != result.Length - 1)
                        ResultAll.Text += result[i] + ",";
                    else
                        ResultAll.Text += result[i];
                }
                ResultAll.Text += "]";
            });
            stopWatch.Stop();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimeExecuteProgram.Text += $"Загальний час виконання програми: {stopWatch.ElapsedMilliseconds} мс";
            });
        }
    }

    private void Swap(ref int a, ref int b)
    {
        var temp = a;
        a = b;
        b = temp;
    }

    private int CountPermutationFactorial(int number)
    {
        int result = 1;
        while (number != 1)
        {
            result *= number;
            number--;
        }
        return result;
    }
}
