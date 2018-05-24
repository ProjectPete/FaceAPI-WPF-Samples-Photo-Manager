# FaceAPI-WPF-Samples-Photo-Manager
Extending the Azure FaceAPI demo solution, to include a photo manager.

Using Congitive Services Face Recognition API, to identify and catalogue the people in my large photo collections.

This project is an extention to the excellent introduction to Mcrosoft Azure's Congitive Services Face Recognition API.

https://github.com/Microsoft/Cognitive-Face-Windows

There is an extra, much larger and completely working scenario added, called "Sort My Photos"

This extra scenario demonstrates and provides a working product, for managing your photo stores.

I will expand the installation and setup instructions asap, but if you know how to clone and compile the project, all you need to do is run the application and do the following:

1) Create a folder with the training images, subfolders for each "person", as shown in the original documentation.

2) Add your API key, as described in the original Azure project notes.

3) Use the "Face Identification" scenario, to load your "person group" into the service.

4) Use the "Sort My Photos" scenario, to start scanning your photo stashes!

5) You can then select a person in a group and click the "Show Images" button, to show all images for a person.

You can also add people to the images manually, just like Facebook image tagging.

As you progress, it gets smarter! 

You can correct mistaken face classifications AND send the mistaken image to the service, which continues to improve the AI for the next images! 

Once the AI is trained with your sample images, you can chug through and tag all your photos, much faster than any other tool I have tried!

Ultimately, once the AI is smart enough, it can process 28,000 a day, on the FREE BAND, which should be more than sufficient for most of us!

I am deeply in love with this feature, and will be working to extend and improve this repo much further.

I have also already started migrating this WPF application over to a Windows Store App, which I will publish for free in the coming weeks.

I am also planning to ask the Product Team to accept this, to be pulled on the repo above. 

I've been tinkering with this for several weeks, but I publish this rough draft early. I've been asked for a demo of how Face Recognition works. So I am publishing this as a "Minimal Viable Product"! :)

Code buffing, style copping and much more documentation will be rolling out over the coming days and weeks.

I plan to make this sample as easy as possible for anyone to pick up and get started with.

Thank you Microsoft for another awesome life changer!

Much more to come!

Priorities:

1) API limit retries needs more tidying.
2) Convert database provider layer into code first, localDb and even an XML/JSON version.
3) More detailed documentation of all the new bits
4) UI tidying & much booiler plating
5) Once the AI is good enough, Add a "first pass" option, which speeds through without user confirmation. It will attenpt to identify or best-guess as many people as it can. The ultimate goal is for this to take 70-80% of the pain out of having to manually tag every image, which other photo apps need you to do.
6) Pull Request on the main/original Azure branch
7) Finish and publish as a free App on Microsoft Store
