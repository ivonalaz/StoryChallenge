import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FetchStoriesComponent } from './fetch-stories.component'
import { Story } from './story';
import { NgxPaginationModule } from 'ngx-pagination';

describe('FetchStoriesComponent', () => {
  let component: FetchStoriesComponent;
  let fixture: ComponentFixture<FetchStoriesComponent>;
  let httpTestingController: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, NgxPaginationModule],
      declarations: [FetchStoriesComponent],
      providers: [{ provide: 'BASE_URL', useValue: 'http://localhost:3000/' }]
    })
      .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FetchStoriesComponent);
    component = fixture.componentInstance;
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should fetch stories on init', () => {
    const stories: Story[] = [
      { title: 'Story 1', url: 'https://someUrl1.com/story1' },
      { title: 'Story 2', url: 'https://someUrl2.com/story2' }
    ];

    component.ngOnInit();

    const req = httpTestingController.expectOne('http://localhost:3000/stories');
    expect(req.request.method).toEqual('GET');

    req.flush(stories);

    expect(component.stories).toEqual(stories);
  });
});
