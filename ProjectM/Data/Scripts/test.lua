function Test(myClass)
    Log(myClass.str)
    Log(myClass.index)

    myClass.str = "new text"
    myClass.index = myClass.index + 5;
end