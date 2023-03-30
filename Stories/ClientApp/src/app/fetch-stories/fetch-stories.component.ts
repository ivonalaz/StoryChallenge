import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Story } from './story'

@Component({
  selector: 'app-fetch-stories',
  templateUrl: './fetch-stories.component.html'
})
export class FetchStoriesComponent {
  public stories: Story[] = [];
  public searchText: string = '';
  public page: any;
  public http: HttpClient;
  public baseUrl: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }

  ngOnInit(): void {
    this.http.get<Story[]>(this.baseUrl + 'stories').subscribe(result => {
      this.stories = result;
    }, error => console.error(error));
  }

  onChange() {
    this.page = 1;
  }
 }

