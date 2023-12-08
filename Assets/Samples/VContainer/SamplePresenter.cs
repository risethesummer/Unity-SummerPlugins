using VContainer.Unity;

namespace Samples.VContainer
{
    public class SamplePresenter : IStartable
    {
        private readonly SampleService _service;
        private readonly SampleView _view;

        public SamplePresenter(SampleService service, SampleView view)
        {
            _service = service;
            _view = view;
        }
        public void Start()
        {
            _view.Btn.onClick.AddListener(_service.SampleDo);
        }
    }
}